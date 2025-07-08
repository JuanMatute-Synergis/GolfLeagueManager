#!/usr/bin/env python3

import psycopg2
from itertools import combinations
import random

# Database connection parameters
conn_params = {
    'host': '192.168.6.67',
    'port': 5432,
    'user': 'golfuser',
    'password': 'golfpassword',
    'database': 'golfdb_southmoore'
}

def get_players():
    """Get all players from the database"""
    conn = psycopg2.connect(**conn_params)
    cursor = conn.cursor()
    
    query = '''
    SELECT "Id", "FirstName" || ' ' || "LastName" as FullName
    FROM "Players"
    ORDER BY "LastName", "FirstName"
    '''
    
    cursor.execute(query)
    players = cursor.fetchall()
    
    cursor.close()
    conn.close()
    
    return players

def get_week1_matchups():
    """Get the existing week 1 matchups"""
    conn = psycopg2.connect(**conn_params)
    cursor = conn.cursor()
    
    query = '''
    SELECT m."PlayerAId", m."PlayerBId"
    FROM "Matchups" m 
    JOIN "Weeks" w ON m."WeekId" = w."Id" 
    WHERE w."WeekNumber" = 1
    '''
    
    cursor.execute(query)
    matchups = cursor.fetchall()
    
    cursor.close()
    conn.close()
    
    return [tuple(sorted([a, b])) for a, b in matchups]

def get_week_id(week_number):
    """Get the week ID for a given week number"""
    conn = psycopg2.connect(**conn_params)
    cursor = conn.cursor()
    
    query = 'SELECT "Id" FROM "Weeks" WHERE "WeekNumber" = %s'
    cursor.execute(query, (week_number,))
    result = cursor.fetchone()
    
    cursor.close()
    conn.close()
    
    return result[0] if result else None

def delete_matchups_weeks_2_to_9():
    """Delete all matchups for weeks 2-9"""
    conn = psycopg2.connect(**conn_params)
    cursor = conn.cursor()
    
    query = '''
    DELETE FROM "Matchups" 
    WHERE "WeekId" IN (
        SELECT "Id" FROM "Weeks" WHERE "WeekNumber" BETWEEN 2 AND 9
    )
    '''
    
    cursor.execute(query)
    deleted_count = cursor.rowcount
    conn.commit()
    
    cursor.close()
    conn.close()
    
    return deleted_count

def round_robin_schedule(players, fixed_week1_pairs):
    """
    Generate a perfect round-robin schedule for 10 players over 9 weeks,
    with week 1 pairs already fixed.
    """
    player_ids = [p[0] for p in players]
    n = len(player_ids)
    
    # All possible pairs
    all_pairs = list(combinations(player_ids, 2))
    all_pairs = [tuple(sorted(pair)) for pair in all_pairs]
    
    # Pairs already used in week 1
    used_pairs = set(fixed_week1_pairs)
    
    # Remaining pairs to schedule
    remaining_pairs = [pair for pair in all_pairs if pair not in used_pairs]
    
    print(f"Total pairs needed: {len(all_pairs)}")
    print(f"Week 1 pairs: {len(fixed_week1_pairs)}")
    print(f"Remaining pairs to schedule: {len(remaining_pairs)}")
    
    # We need to schedule remaining_pairs across weeks 2-9
    # Each week has 5 matches, so 8 weeks * 5 matches = 40 matches total
    weeks_needed = 8
    matches_per_week = 5
    
    if len(remaining_pairs) != weeks_needed * matches_per_week:
        print(f"❌ Math error: Need {weeks_needed * matches_per_week} pairs but have {len(remaining_pairs)}")
        return None
    
    # Generate schedule using a systematic approach
    schedule = []
    available_pairs = remaining_pairs.copy()
    
    for week in range(2, 10):  # Weeks 2-9
        week_matches = []
        week_players = set()
        
        # Try to find 5 non-overlapping pairs for this week
        week_pairs = []
        remaining_for_week = available_pairs.copy()
        
        # Use a greedy approach with backtracking if needed
        attempts = 0
        max_attempts = 1000
        
        while len(week_pairs) < 5 and attempts < max_attempts:
            attempts += 1
            
            if not remaining_for_week:
                # If we run out of pairs, we need to backtrack
                if week_pairs:
                    # Remove the last pair and try again
                    last_pair = week_pairs.pop()
                    remaining_for_week.append(last_pair)
                    week_players.discard(last_pair[0])
                    week_players.discard(last_pair[1])
                    continue
                else:
                    # Complete failure, try reshuffling
                    random.shuffle(available_pairs)
                    remaining_for_week = available_pairs.copy()
                    week_pairs = []
                    week_players = set()
                    continue
            
            # Find a pair that doesn't conflict with already selected players
            pair_found = False
            for i, pair in enumerate(remaining_for_week):
                if pair[0] not in week_players and pair[1] not in week_players:
                    week_pairs.append(pair)
                    week_players.add(pair[0])
                    week_players.add(pair[1])
                    remaining_for_week.pop(i)
                    pair_found = True
                    break
            
            if not pair_found:
                # No valid pair found, backtrack
                if week_pairs:
                    last_pair = week_pairs.pop()
                    remaining_for_week.append(last_pair)
                    week_players.discard(last_pair[0])
                    week_players.discard(last_pair[1])
                else:
                    # Try random shuffle
                    random.shuffle(available_pairs)
                    remaining_for_week = available_pairs.copy()
                    week_pairs = []
                    week_players = set()
        
        if len(week_pairs) == 5:
            schedule.append((week, week_pairs))
            # Remove used pairs from available pairs
            for pair in week_pairs:
                if pair in available_pairs:
                    available_pairs.remove(pair)
        else:
            print(f"❌ Failed to generate week {week} (only got {len(week_pairs)} pairs)")
            return None
    
    return schedule

def insert_matchups(schedule):
    """Insert the new matchups into the database"""
    conn = psycopg2.connect(**conn_params)
    cursor = conn.cursor()
    
    total_inserted = 0
    
    for week_number, pairs in schedule:
        week_id = get_week_id(week_number)
        if not week_id:
            print(f"❌ Could not find week {week_number}")
            continue
        
        for player_a_id, player_b_id in pairs:
            query = '''
            INSERT INTO "Matchups" ("WeekId", "PlayerAId", "PlayerBId")
            VALUES (%s, %s, %s)
            '''
            cursor.execute(query, (week_id, player_a_id, player_b_id))
            total_inserted += 1
    
    conn.commit()
    cursor.close()
    conn.close()
    
    return total_inserted

def main():
    print("🔄 Generating Perfect Round-Robin Schedule")
    print("=" * 50)
    
    # Get data
    players = get_players()
    week1_pairs = get_week1_matchups()
    
    print(f"Players: {len(players)}")
    print(f"Week 1 pairs: {len(week1_pairs)}")
    
    # Delete existing weeks 2-9
    deleted = delete_matchups_weeks_2_to_9()
    print(f"Deleted {deleted} existing matchups for weeks 2-9")
    
    # Generate new schedule
    print("\n🎯 Generating new schedule...")
    
    # Try multiple times with different random seeds if needed
    schedule = None
    for attempt in range(10):
        random.seed(attempt)
        schedule = round_robin_schedule(players, week1_pairs)
        if schedule:
            break
        print(f"Attempt {attempt + 1} failed, trying again...")
    
    if not schedule:
        print("❌ Failed to generate a valid schedule after multiple attempts")
        return False
    
    print("✅ Schedule generated successfully!")
    
    # Insert into database
    print("\n💾 Inserting matchups into database...")
    inserted = insert_matchups(schedule)
    print(f"Inserted {inserted} new matchups")
    
    # Print the schedule
    print("\n📅 New Schedule (Weeks 2-9):")
    print("-" * 40)
    
    # Get player names for display
    player_names = {p[0]: p[1] for p in players}
    
    for week_number, pairs in schedule:
        print(f"\nWeek {week_number}:")
        for player_a_id, player_b_id in pairs:
            player_a = player_names[player_a_id]
            player_b = player_names[player_b_id]
            print(f"  {player_a} vs {player_b}")
    
    return True

if __name__ == "__main__":
    try:
        success = main()
        if success:
            print("\n🎉 Perfect round-robin schedule generated successfully!")
            print("Run analyze_round_robin.py to verify the results.")
        else:
            print("\n❌ Failed to generate schedule")
    except Exception as e:
        print(f"❌ Error: {e}")
        import traceback
        traceback.print_exc()
