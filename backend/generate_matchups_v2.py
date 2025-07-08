#!/usr/bin/env python3
"""
Generate golf league matchups for weeks 2-9, ensuring ALL players play every week.
Week 1 matchups are preserved and cannot be repeated.
"""

import psycopg2
import uuid
from itertools import combinations
import random

# Database connection
DB_CONFIG = {
    'host': '192.168.6.67',
    'port': 5432,
    'database': 'golfdb_southmoore',
    'user': 'golfuser',
    'password': 'golfpassword'
}

def get_database_connection():
    """Get database connection"""
    return psycopg2.connect(**DB_CONFIG)

def get_players_and_weeks():
    """Get all players and week information"""
    conn = get_database_connection()
    cur = conn.cursor()
    
    # Get all players
    cur.execute('SELECT "Id", "FirstName", "LastName" FROM "Players" ORDER BY "LastName"')
    players = [(row[0], f"{row[1]} {row[2]}") for row in cur.fetchall()]
    
    # Get weeks 2-9
    cur.execute('SELECT "Id", "WeekNumber" FROM "Weeks" WHERE "WeekNumber" BETWEEN 2 AND 9 ORDER BY "WeekNumber"')
    weeks = cur.fetchall()
    
    # Get Week 1 matchups (to avoid repeating)
    cur.execute('''
        SELECT m."PlayerAId", m."PlayerBId"
        FROM "Matchups" m
        JOIN "Weeks" w ON m."WeekId" = w."Id"
        WHERE w."WeekNumber" = 1
    ''')
    week1_matchups = set()
    for row in cur.fetchall():
        # Store both directions of the matchup
        week1_matchups.add((row[0], row[1]))
        week1_matchups.add((row[1], row[0]))
    
    cur.close()
    conn.close()
    
    return players, weeks, week1_matchups

def clear_existing_matchups():
    """Clear existing matchups for weeks 2-9"""
    conn = get_database_connection()
    cur = conn.cursor()
    
    try:
        cur.execute('''
            DELETE FROM "Matchups" WHERE "WeekId" IN (
                SELECT "Id" FROM "Weeks" WHERE "WeekNumber" BETWEEN 2 AND 9
            )
        ''')
        deleted_count = cur.rowcount
        conn.commit()
        print(f"Cleared {deleted_count} existing matchups from weeks 2-9")
    except Exception as e:
        conn.rollback()
        print(f"Error clearing matchups: {e}")
        raise
    finally:
        cur.close()
        conn.close()

def generate_round_robin_schedule(players, weeks, week1_matchups):
    """Generate round-robin schedule ensuring ALL players play every week"""
    player_ids = [p[0] for p in players]
    player_names = {p[0]: p[1] for p in players}
    
    # With 10 players, we need exactly 5 matchups per week
    assert len(player_ids) == 10, f"Expected 10 players, got {len(player_ids)}"
    
    print(f"Total players: {len(player_ids)}")
    print(f"Players: {[player_names[pid] for pid in player_ids]}")
    
    # Generate all possible pairings
    all_possible_pairings = list(combinations(player_ids, 2))
    
    # Remove Week 1 matchups from available pairings
    available_pairings = []
    for pair in all_possible_pairings:
        if pair not in week1_matchups and (pair[1], pair[0]) not in week1_matchups:
            available_pairings.append(pair)
    
    print(f"Total possible pairings: {len(all_possible_pairings)}")
    print(f"Week 1 matchups (unavailable): {len(week1_matchups) // 2}")
    print(f"Available pairings for weeks 2-9: {len(available_pairings)}")
    
    # Track which players have played each other
    played_pairs = set(week1_matchups)
    
    # Track how many times each player has played
    player_games = {pid: 1 for pid in player_ids}  # Start with 1 for Week 1
    
    schedule = {}
    
    # For each week, create exactly 5 matchups using all 10 players
    for week_id, week_number in weeks:
        week_matchups = []
        used_players = set()
        remaining_players = set(player_ids)
        
        print(f"\nWeek {week_number}:")
        
        # Get available pairings for this week (not yet played)
        week_available_pairings = [p for p in available_pairings if p not in played_pairs and (p[1], p[0]) not in played_pairs]
        
        # Sort by players who have played least
        week_available_pairings.sort(key=lambda x: player_games[x[0]] + player_games[x[1]])
        
        print(f"  Available unique pairings: {len(week_available_pairings)}")
        
        # Try to make exactly 5 matchups using all 10 players
        attempts = 0
        max_attempts = 5000
        
        while len(week_matchups) < 5 and len(remaining_players) >= 2 and attempts < max_attempts:
            attempts += 1
            
            # If we have exactly 2 players left, pair them
            if len(remaining_players) == 2:
                players_left = list(remaining_players)
                pair = tuple(sorted(players_left))
                week_matchups.append(pair)
                remaining_players.clear()
                
                # Mark this pair as played
                played_pairs.add(pair)
                played_pairs.add((pair[1], pair[0]))
                player_games[pair[0]] += 1
                player_games[pair[1]] += 1
                
                print(f"  Added (final pair): {player_names[pair[0]]} vs {player_names[pair[1]]}")
                break
            
            # Find the best pairing from remaining players
            best_pair = None
            best_score = float('inf')  # Lower is better (prefer new pairings)
            
            for p1 in remaining_players:
                for p2 in remaining_players:
                    if p1 >= p2:  # Avoid duplicates and self-pairing
                        continue
                    
                    pair = (p1, p2)
                    reverse_pair = (p2, p1)
                    
                    # Calculate "newness" score - prefer pairings that haven't played
                    if pair in played_pairs or reverse_pair in played_pairs:
                        newness_score = 1000  # High penalty for repeated pairings
                    else:
                        newness_score = 0  # Best score for new pairings
                    
                    # Add slight preference for balancing game counts
                    balance_score = abs(player_games[p1] - player_games[p2])
                    
                    total_score = newness_score + balance_score
                    
                    if total_score < best_score:
                        best_score = total_score
                        best_pair = pair
            
            if best_pair is None:
                print(f"  Warning: No valid pair found at attempt {attempts}")
                break
            
            # Add the best pair
            week_matchups.append(best_pair)
            remaining_players.remove(best_pair[0])
            remaining_players.remove(best_pair[1])
            
            # Mark this pair as played (only if it's a new pairing)
            if best_score < 1000:  # Only if it wasn't a repeated pairing
                played_pairs.add(best_pair)
                played_pairs.add((best_pair[1], best_pair[0]))
            
            player_games[best_pair[0]] += 1
            player_games[best_pair[1]] += 1
            
            is_repeat = "(REPEAT)" if best_score >= 1000 else ""
            print(f"  Added: {player_names[best_pair[0]]} vs {player_names[best_pair[1]]} {is_repeat}")
        
        if len(week_matchups) != 5:
            print(f"  ERROR: Week {week_number} has {len(week_matchups)} matchups instead of 5!")
            print(f"  Remaining players: {[player_names[pid] for pid in remaining_players]}")
        
        if len(remaining_players) > 0:
            print(f"  ERROR: {len(remaining_players)} players not assigned: {[player_names[pid] for pid in remaining_players]}")
        
        schedule[week_id] = week_matchups
    
    # Print summary
    print(f"\nPlayer game counts after scheduling:")
    for pid in player_ids:
        print(f"  {player_names[pid]}: {player_games[pid]} games")
    
    return schedule

def insert_matchups(schedule):
    """Insert the generated matchups into the database"""
    conn = get_database_connection()
    cur = conn.cursor()
    
    try:
        total_inserted = 0
        for week_id, matchups in schedule.items():
            for player_a_id, player_b_id in matchups:
                matchup_id = str(uuid.uuid4())
                
                cur.execute('''
                    INSERT INTO "Matchups" (
                        "Id", "WeekId", "PlayerAId", "PlayerBId",
                        "PlayerAScore", "PlayerBScore", "PlayerAPoints", "PlayerBPoints",
                        "PlayerAHolePoints", "PlayerBHolePoints", "PlayerAMatchWin", "PlayerBMatchWin",
                        "PlayerAAbsent", "PlayerBAbsent", "PlayerAAbsentWithNotice", "PlayerBAbsentWithNotice"
                    ) VALUES (
                        %s, %s, %s, %s,
                        NULL, NULL, NULL, NULL,
                        0, 0, FALSE, FALSE,
                        FALSE, FALSE, FALSE, FALSE
                    )
                ''', (matchup_id, week_id, player_a_id, player_b_id))
                total_inserted += 1
        
        conn.commit()
        print(f"\nSuccessfully inserted {total_inserted} matchups")
        
    except Exception as e:
        conn.rollback()
        print(f"Error inserting matchups: {e}")
        raise
    finally:
        cur.close()
        conn.close()

def main():
    """Main function"""
    print("Golf League Matchup Generator v2.0")
    print("=" * 50)
    
    # Get data from database
    players, weeks, week1_matchups = get_players_and_weeks()
    
    # Clear existing matchups for weeks 2-9
    clear_existing_matchups()
    
    # Generate schedule ensuring everyone plays every week
    schedule = generate_round_robin_schedule(players, weeks, week1_matchups)
    
    # Insert into database
    insert_matchups(schedule)
    
    print("\nMatchup generation complete!")

if __name__ == "__main__":
    main()
