#!/usr/bin/env python3
"""
Generate a perfect round-robin schedule for 10 players over 9 weeks.
Each player plays each other player exactly once.
"""

import psycopg2
from psycopg2.extras import RealDictCursor
import sys

def connect_to_db():
    """Connect to PostgreSQL database"""
    try:
        conn = psycopg2.connect(
            host="localhost",
            database="golfdb_htlyons",
            user="postgres",
            password="admin123"
        )
        return conn
    except Exception as e:
        print(f"Error connecting to database: {e}")
        sys.exit(1)

def generate_round_robin_schedule(players):
    """
    Generate a perfect round-robin schedule using the standard algorithm.
    For n players, we need n-1 rounds if n is even.
    """
    n = len(players)
    if n % 2 != 0:
        raise ValueError("Number of players must be even for round-robin")
    
    # We'll use the standard round-robin algorithm
    # Fix one player (player 0) and rotate the others
    schedule = []
    
    for round_num in range(n - 1):  # n-1 rounds for n players
        round_matches = []
        
        # Create the arrangement for this round
        # Player 0 is fixed, others rotate
        arrangement = [players[0]]  # Fixed player
        
        # Add the rotating players
        for i in range(1, n):
            # Calculate position after rotation
            pos = (i - round_num - 1) % (n - 1) + 1
            arrangement.append(players[pos])
        
        # Pair up players: first with last, second with second-to-last, etc.
        for i in range(n // 2):
            player1 = arrangement[i]
            player2 = arrangement[n - 1 - i]
            round_matches.append((player1, player2))
        
        schedule.append(round_matches)
    
    return schedule

def get_players_and_weeks():
    """Get players and week data from database"""
    conn = connect_to_db()
    cursor = conn.cursor(cursor_factory=RealDictCursor)
    
    try:
        # Get all players
        cursor.execute("SELECT id, firstname, lastname FROM players ORDER BY lastname, firstname")
        players = cursor.fetchall()
        
        if len(players) != 10:
            print(f"Expected 10 players, found {len(players)}")
            return None, None
        
        # Get weeks 2-9 (we're keeping week 1 intact)
        cursor.execute("""
            SELECT id, weeknumber 
            FROM weeks 
            WHERE weeknumber BETWEEN 2 AND 9 
            ORDER BY weeknumber
        """)
        weeks = cursor.fetchall()
        
        if len(weeks) != 8:
            print(f"Expected 8 weeks (2-9), found {len(weeks)}")
            return None, None
        
        return players, weeks
    
    finally:
        cursor.close()
        conn.close()

def insert_matchups(schedule, players, weeks):
    """Insert the generated matchups into the database"""
    conn = connect_to_db()
    cursor = conn.cursor()
    
    try:
        # Delete existing matchups for weeks 2-9
        cursor.execute("DELETE FROM matchups WHERE weekid IN (SELECT id FROM weeks WHERE weeknumber BETWEEN 2 AND 9)")
        deleted_count = cursor.rowcount
        print(f"Deleted {deleted_count} existing matchups for weeks 2-9")
        
        # Insert new matchups
        insert_count = 0
        for round_idx, round_matches in enumerate(schedule):
            week = weeks[round_idx]  # weeks[0] = week 2, etc.
            week_number = week['weeknumber']
            week_id = week['id']
            
            print(f"\nWeek {week_number}:")
            for match in round_matches:
                player1_id, player2_id = match
                
                # Find player names for display
                p1 = next(p for p in players if p['id'] == player1_id)
                p2 = next(p for p in players if p['id'] == player2_id)
                print(f"  {p1['firstname']} {p1['lastname']} vs {p2['firstname']} {p2['lastname']}")
                
                # Insert matchup
                cursor.execute("""
                    INSERT INTO matchups (weekid, playeraid, playerbid, createdat, updatedat)
                    VALUES (%s, %s, %s, NOW(), NOW())
                """, (week_id, player1_id, player2_id))
                insert_count += 1
        
        conn.commit()
        print(f"\nSuccessfully inserted {insert_count} new matchups")
        
    except Exception as e:
        conn.rollback()
        print(f"Error inserting matchups: {e}")
        raise
    
    finally:
        cursor.close()
        conn.close()

def verify_schedule(schedule, players):
    """Verify that the schedule is a perfect round-robin"""
    n = len(players)
    player_opponents = {p['id']: set() for p in players}
    
    total_matches = 0
    for round_matches in schedule:
        for p1_id, p2_id in round_matches:
            player_opponents[p1_id].add(p2_id)
            player_opponents[p2_id].add(p1_id)
            total_matches += 1
    
    # Verify each player plays each other player exactly once
    perfect = True
    for player in players:
        player_id = player['id']
        opponents = player_opponents[player_id]
        expected_opponents = {p['id'] for p in players if p['id'] != player_id}
        
        if opponents != expected_opponents:
            print(f"ERROR: {player['firstname']} {player['lastname']} has incorrect opponents")
            missing = expected_opponents - opponents
            extra = opponents - expected_opponents
            if missing:
                missing_names = [f"{p['firstname']} {p['lastname']}" for p in players if p['id'] in missing]
                print(f"  Missing: {missing_names}")
            if extra:
                extra_names = [f"{p['firstname']} {p['lastname']}" for p in players if p['id'] in extra]
                print(f"  Extra: {extra_names}")
            perfect = False
    
    expected_total = n * (n - 1) // 2  # Total unique pairs
    if total_matches != expected_total:
        print(f"ERROR: Expected {expected_total} total matches, got {total_matches}")
        perfect = False
    
    if perfect:
        print("✓ Perfect round-robin verified!")
        print(f"✓ {len(schedule)} rounds with {len(schedule[0])} matches each")
        print(f"✓ {total_matches} total matches")
        print(f"✓ Each player plays each other player exactly once")
    
    return perfect

def main():
    print("Generating perfect round-robin schedule for 10 players over 9 weeks...")
    
    # Get data from database
    players, weeks = get_players_and_weeks()
    if not players or not weeks:
        return
    
    print(f"\nPlayers ({len(players)}):")
    for i, player in enumerate(players):
        print(f"  {i}: {player['firstname']} {player['lastname']}")
    
    # Generate the schedule
    try:
        player_ids = [p['id'] for p in players]
        schedule = generate_round_robin_schedule(player_ids)
        
        # Verify the schedule is perfect
        if not verify_schedule(schedule, players):
            print("Schedule generation failed verification!")
            return
        
        # Insert into database
        insert_matchups(schedule, players, weeks)
        
        print("\n" + "="*50)
        print("Perfect round-robin schedule generated successfully!")
        print("Each player will play each other player exactly once.")
        print("="*50)
        
    except Exception as e:
        print(f"Error generating schedule: {e}")

if __name__ == "__main__":
    main()
