#!/usr/bin/env python3

import psycopg2
from itertools import combinations

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

def generate_remaining_schedule(players, week1_pairs):
    """
    Generate a schedule for weeks 2-9 that completes the round-robin
    starting from the fixed week 1 pairs.
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
    
    # We need to arrange remaining_pairs into 8 weeks (2-9) with 5 matches each
    if len(remaining_pairs) != 40:  # 8 weeks * 5 matches
        print(f"❌ Math error: Expected 40 remaining pairs, got {len(remaining_pairs)}")
        return None
    
    # Use a more sophisticated backtracking algorithm
    schedule = []
    
    def is_valid_week(week_pairs, all_players):
        """Check if a week's pairings use each player exactly once"""
        used_players = set()
        for p1, p2 in week_pairs:
            if p1 in used_players or p2 in used_players:
                return False
            used_players.add(p1)
            used_players.add(p2)
        return len(used_players) == len(all_players)
    
    def backtrack(week_num, available_pairs, current_schedule):
        """Recursive backtracking to find valid schedule"""
        if week_num > 9:  # Successfully scheduled all weeks
            return current_schedule
        
        if week_num == 1:  # Skip week 1 (already fixed)
            return backtrack(week_num + 1, available_pairs, current_schedule)
        
        # Try to find a valid set of 5 pairs for this week
        for week_pairs in combinations(available_pairs, 5):
            if is_valid_week(week_pairs, player_ids):
                # This is a valid week, try to continue
                new_available = [p for p in available_pairs if p not in week_pairs]
                new_schedule = current_schedule + [(week_num, list(week_pairs))]
                
                result = backtrack(week_num + 1, new_available, new_schedule)
                if result is not None:
                    return result
        
        return None  # No valid schedule found
    
    print("🔍 Searching for valid schedule using backtracking...")
    print("This may take a moment...")
    
    # Start backtracking from week 2
    result = backtrack(2, remaining_pairs, [])
    
    if result is None:
        print("❌ Could not find a valid schedule")
        return None
    
    print(f"✅ Found valid schedule with {len(result)} weeks")
    return result

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
    print("🔄 Generating Custom Round-Robin Schedule (V5 - Backtracking)")
    print("=" * 65)
    
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
    
    # Generate remaining schedule
    print("\n🎯 Generating weeks 2-9...")
    schedule = generate_remaining_schedule(players, week1_pairs)
    
    if not schedule:
        print("❌ Failed to generate schedule")
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
            print("\n🎉 Custom round-robin schedule generated successfully!")
            print("Run analyze_round_robin.py to verify the results.")
        else:
            print("\n❌ Failed to generate schedule")
    except Exception as e:
        print(f"❌ Error: {e}")
        import traceback
        traceback.print_exc()
