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

def generate_smart_schedule(players, week1_pairs):
    """
    Generate a schedule using a smart greedy approach with constraint satisfaction
    """
    player_ids = [p[0] for p in players]
    
    # All possible pairs
    all_pairs = list(combinations(player_ids, 2))
    all_pairs = [tuple(sorted(pair)) for pair in all_pairs]
    
    # Pairs already used in week 1
    used_pairs = set(week1_pairs)
    
    # Remaining pairs to schedule
    remaining_pairs = [pair for pair in all_pairs if pair not in used_pairs]
    
    print(f"Total possible pairs: {len(all_pairs)}")
    print(f"Week 1 pairs: {len(week1_pairs)}")
    print(f"Remaining pairs to schedule: {len(remaining_pairs)}")
    
    # Track how many times each player has been scheduled in each week
    player_week_count = {pid: {week: 0 for week in range(2, 10)} for pid in player_ids}
    
    # Generate schedule week by week
    schedule = []
    available_pairs = remaining_pairs.copy()
    
    for week in range(2, 10):  # Weeks 2-9
        print(f"\n🔍 Generating week {week}...")
        week_pairs = []
        week_players = set()
        temp_available = available_pairs.copy()
        
        # Sort pairs by player frequency (prefer players who haven't played much)
        def pair_priority(pair):
            p1, p2 = pair
            # Priority: lower total games played so far
            p1_games = sum(player_week_count[p1][w] for w in range(2, week))
            p2_games = sum(player_week_count[p2][w] for w in range(2, week))
            return p1_games + p2_games
        
        temp_available.sort(key=pair_priority)
        
        # Greedily select 5 non-conflicting pairs
        for pair in temp_available[:]:
            if len(week_pairs) >= 5:
                break
            
            p1, p2 = pair
            if p1 not in week_players and p2 not in week_players:
                week_pairs.append(pair)
                week_players.add(p1)
                week_players.add(p2)
                temp_available.remove(pair)
                player_week_count[p1][week] = 1
                player_week_count[p2][week] = 1
        
        if len(week_pairs) != 5:
            print(f"❌ Could not find 5 pairs for week {week}, only found {len(week_pairs)}")
            
            # Try random shuffling as backup
            for attempt in range(100):
                random.shuffle(available_pairs)
                test_pairs = []
                test_players = set()
                
                for pair in available_pairs:
                    if len(test_pairs) >= 5:
                        break
                    p1, p2 = pair
                    if p1 not in test_players and p2 not in test_players:
                        test_pairs.append(pair)
                        test_players.add(p1)
                        test_players.add(p2)
                
                if len(test_pairs) == 5:
                    week_pairs = test_pairs
                    print(f"✅ Found solution with random attempt {attempt + 1}")
                    break
            
            if len(week_pairs) != 5:
                return None
        
        # Remove used pairs from available pairs
        for pair in week_pairs:
            if pair in available_pairs:
                available_pairs.remove(pair)
        
        schedule.append((week, week_pairs))
        print(f"✅ Week {week} scheduled with {len(week_pairs)} matches")
    
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
    print("🔄 Generating Smart Round-Robin Schedule (V6 - Constraint Satisfaction)")
    print("=" * 70)
    
    # Get data
    players = get_players()
    week1_pairs = get_week1_matchups()
    
    print(f"Players: {len(players)}")
    print(f"Week 1 pairs: {len(week1_pairs)}")
    
    # Show week 1 matchups
    player_names = {p[0]: p[1] for p in players}
    print("\nWeek 1 (Fixed):")
    for player_a_id, player_b_id in week1_pairs:
        player_a = player_names[player_a_id]
        player_b = player_names[player_b_id]
        print(f"  {player_a} vs {player_b}")
    
    # Try multiple random seeds to find a solution
    schedule = None
    for attempt in range(20):
        random.seed(attempt)
        print(f"\n🎯 Attempt {attempt + 1} to generate weeks 2-9...")
        schedule = generate_smart_schedule(players, week1_pairs)
        
        if schedule:
            print(f"✅ Success on attempt {attempt + 1}!")
            break
        else:
            print(f"❌ Attempt {attempt + 1} failed")
    
    if not schedule:
        print("❌ Failed to generate schedule after multiple attempts")
        return False
    
    # Delete existing weeks 2-9
    deleted = delete_matchups_weeks_2_to_9()
    print(f"\nDeleted {deleted} existing matchups for weeks 2-9")
    
    # Insert new schedule
    print(f"💾 Inserting {len(schedule)} weeks into database...")
    inserted = insert_matchups(schedule)
    print(f"Inserted {inserted} new matchups")
    
    # Print the complete schedule
    print("\n📅 New Schedule (Weeks 2-9):")
    print("-" * 40)
    
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
            print("\n🎉 Smart round-robin schedule generated successfully!")
            print("Run analyze_round_robin.py to verify the results.")
        else:
            print("\n❌ Failed to generate schedule")
    except Exception as e:
        print(f"❌ Error: {e}")
        import traceback
        traceback.print_exc()
