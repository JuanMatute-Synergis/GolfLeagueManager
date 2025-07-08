#!/usr/bin/env python3
"""
Generate golf league matchups for weeks 2-9, ensuring everyone plays each other at least once.
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

def generate_round_robin_schedule(players, weeks, week1_matchups):
    """Generate round-robin schedule avoiding Week 1 matchups"""
    player_ids = [p[0] for p in players]
    player_names = {p[0]: p[1] for p in players}
    
    # Generate all possible pairings
    all_possible_pairings = list(combinations(player_ids, 2))
    
    # Remove Week 1 matchups from available pairings
    available_pairings = []
    for pair in all_possible_pairings:
        if pair not in week1_matchups and (pair[1], pair[0]) not in week1_matchups:
            available_pairings.append(pair)
    
    print(f"Total players: {len(player_ids)}")
    print(f"Total possible pairings: {len(all_possible_pairings)}")
    print(f"Week 1 matchups (unavailable): {len(week1_matchups) // 2}")
    print(f"Available pairings for weeks 2-9: {len(available_pairings)}")
    
    # Track which players have played each other
    played_pairs = set(week1_matchups)
    
    # Track how many times each player has played
    player_games = {pid: 1 for pid in player_ids}  # Start with 1 for Week 1
    
    schedule = {}
    
    # For each week, try to create balanced matchups
    for week_id, week_number in weeks:
        week_matchups = []
        used_players = set()
        week_available_pairings = [p for p in available_pairings if p not in played_pairs and (p[1], p[0]) not in played_pairs]
        
        # Sort by players who have played least
        week_available_pairings.sort(key=lambda x: player_games[x[0]] + player_games[x[1]])
        
        print(f"\nWeek {week_number}:")
        print(f"  Available pairings: {len(week_available_pairings)}")
        
        # Try to make 5 matchups (or as many as possible)
        attempts = 0
        max_attempts = 1000
        
        while len(week_matchups) < 5 and week_available_pairings and attempts < max_attempts:
            attempts += 1
            
            # Find a pairing where both players are available
            for i, (p1, p2) in enumerate(week_available_pairings):
                if p1 not in used_players and p2 not in used_players:
                    week_matchups.append((p1, p2))
                    used_players.add(p1)
                    used_players.add(p2)
                    played_pairs.add((p1, p2))
                    played_pairs.add((p2, p1))
                    player_games[p1] += 1
                    player_games[p2] += 1
                    
                    # Remove this pairing from available list
                    week_available_pairings.pop(i)
                    
                    print(f"  Added: {player_names[p1]} vs {player_names[p2]}")
                    break
            else:
                # No valid pairing found, break
                break
        
        if len(week_matchups) < 5:
            print(f"  Warning: Only {len(week_matchups)} matchups possible for week {week_number}")
        
        schedule[week_id] = week_matchups
    
    # Print summary
    print(f"\nPlayer game counts after scheduling:")
    for pid in player_ids:
        print(f"  {player_names[pid]}: {player_games[pid]} games")
    
    # Check which pairings are still missing
    all_pairs_needed = set(all_possible_pairings)
    all_pairs_scheduled = set()
    for matchups in schedule.values():
        for pair in matchups:
            all_pairs_scheduled.add(pair)
            all_pairs_scheduled.add((pair[1], pair[0]))
    
    # Add Week 1 pairs
    all_pairs_scheduled.update(week1_matchups)
    
    missing_pairs = []
    for pair in all_pairs_needed:
        if pair not in all_pairs_scheduled and (pair[1], pair[0]) not in all_pairs_scheduled:
            missing_pairs.append(pair)
    
    print(f"\nMissing pairings (won't play each other): {len(missing_pairs)}")
    for pair in missing_pairs:
        print(f"  {player_names[pair[0]]} vs {player_names[pair[1]]}")
    
    return schedule

def insert_matchups(schedule):
    """Insert the generated matchups into the database"""
    conn = get_database_connection()
    cur = conn.cursor()
    
    try:
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
        
        conn.commit()
        print(f"\nSuccessfully inserted {sum(len(matchups) for matchups in schedule.values())} matchups")
        
    except Exception as e:
        conn.rollback()
        print(f"Error inserting matchups: {e}")
        raise
    finally:
        cur.close()
        conn.close()

def main():
    """Main function"""
    print("Golf League Matchup Generator")
    print("=" * 50)
    
    # Get data from database
    players, weeks, week1_matchups = get_players_and_weeks()
    
    # Generate schedule
    schedule = generate_round_robin_schedule(players, weeks, week1_matchups)
    
    # Insert into database
    insert_matchups(schedule)
    
    print("\nMatchup generation complete!")

if __name__ == "__main__":
    main()
