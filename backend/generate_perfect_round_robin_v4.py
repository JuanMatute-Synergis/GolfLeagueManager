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

def generate_complete_round_robin(players):
    """
    Generate a complete round-robin tournament for 10 players over 9 weeks
    using the circle method algorithm.
    """
    player_ids = [p[0] for p in players]
    n = len(player_ids)
    
    if n % 2 != 0:
        raise ValueError("Round-robin requires even number of players")
    
    # Generate all rounds using circle method
    rounds = []
    
    # Fix one player (index 0) and rotate others
    fixed_player = player_ids[0]
    rotating_players = player_ids[1:]
    
    for round_num in range(n - 1):  # 9 rounds for 10 players
        round_matches = []
        
        # Current rotation
        current_rotation = rotating_players[:]
        
        # Pair fixed player with first in rotation
        round_matches.append(tuple(sorted([fixed_player, current_rotation[0]])))
        
        # Pair remaining players
        for i in range(1, len(current_rotation) // 2 + 1):
            if i < len(current_rotation) - i + 1:
                pair = tuple(sorted([current_rotation[i], current_rotation[-i]]))
                round_matches.append(pair)
        
        rounds.append(round_matches)
        
        # Rotate for next round (move last to first)
        rotating_players = [rotating_players[-1]] + rotating_players[:-1]
    
    return rounds

def find_matching_round(complete_rounds, week1_pairs):
    """
    Find which round in the complete round-robin matches week 1.
    """
    week1_set = set(week1_pairs)
    
    for i, round_matches in enumerate(complete_rounds):
        round_set = set(round_matches)
        if round_set == week1_set:
            return i
    
    return None

def reorder_rounds_from_week1(complete_rounds, week1_round_index):
    """
    Reorder the complete rounds so that week1 is first and others follow.
    """
    if week1_round_index is None:
        return None
    
    # Reorder: week1 first, then remaining rounds
    reordered = [complete_rounds[week1_round_index]]  # Week 1
    
    # Add remaining rounds (weeks 2-9)
    for i in range(len(complete_rounds)):
        if i != week1_round_index:
            reordered.append(complete_rounds[i])
    
    return reordered

def insert_matchups(schedule_weeks_2_to_9):
    """Insert the new matchups into the database for weeks 2-9"""
    conn = psycopg2.connect(**conn_params)
    cursor = conn.cursor()
    
    total_inserted = 0
    
    for week_offset, pairs in enumerate(schedule_weeks_2_to_9):
        week_number = week_offset + 2  # Start from week 2
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
    print("🔄 Generating Perfect Round-Robin Schedule (Algorithm V4)")
    print("=" * 60)
    
    # Get data
    players = get_players()
    week1_pairs = get_week1_matchups()
    
    print(f"Players: {len(players)}")
    print(f"Week 1 pairs: {len(week1_pairs)}")
    
    # Generate complete round-robin
    print("\n🎯 Generating complete round-robin...")
    complete_rounds = generate_complete_round_robin(players)
    
    print(f"Generated {len(complete_rounds)} rounds")
    print(f"Each round has {len(complete_rounds[0])} matches")
    
    # Find which round matches week 1
    print("\n🔍 Finding week 1 in the complete schedule...")
    week1_round_index = find_matching_round(complete_rounds, week1_pairs)
    
    if week1_round_index is None:
        print("❌ Week 1 matchups don't match any round in the generated schedule!")
        print(f"Week 1 pairs: {week1_pairs}")
        print("Generated rounds:")
        for i, round_matches in enumerate(complete_rounds):
            print(f"  Round {i+1}: {round_matches}")
        return False
    
    print(f"✅ Week 1 matches round {week1_round_index + 1} in the complete schedule")
    
    # Reorder rounds to start from week 1
    reordered_rounds = reorder_rounds_from_week1(complete_rounds, week1_round_index)
    
    # Delete existing weeks 2-9
    deleted = delete_matchups_weeks_2_to_9()
    print(f"Deleted {deleted} existing matchups for weeks 2-9")
    
    # Insert weeks 2-9 (skip the first round which is week 1)
    weeks_2_to_9 = reordered_rounds[1:]  # Skip week 1
    
    print(f"\n💾 Inserting {len(weeks_2_to_9)} weeks into database...")
    inserted = insert_matchups(weeks_2_to_9)
    print(f"Inserted {inserted} new matchups")
    
    # Print the schedule
    print("\n📅 Complete Schedule:")
    print("-" * 40)
    
    # Get player names for display
    player_names = {p[0]: p[1] for p in players}
    
    for week_num, round_matches in enumerate(reordered_rounds):
        print(f"\nWeek {week_num + 1}:")
        for player_a_id, player_b_id in round_matches:
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
