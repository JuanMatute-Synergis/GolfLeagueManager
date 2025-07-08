#!/usr/bin/env python3

import psycopg2
from collections import defaultdict

# Database connection parameters
conn_params = {
    'host': '192.168.6.67',
    'port': 5432,
    'user': 'golfuser',
    'password': 'golfpassword',
    'database': 'golfdb_southmoore'
}

def find_duplicates():
    """Find which pairings are duplicated"""
    conn = psycopg2.connect(**conn_params)
    cursor = conn.cursor()
    
    query = '''
    SELECT 
        w."WeekNumber",
        pa."FirstName" || ' ' || pa."LastName" as PlayerA,
        pb."FirstName" || ' ' || pb."LastName" as PlayerB,
        pa."Id" as PlayerAId,
        pb."Id" as PlayerBId
    FROM "Matchups" m 
    JOIN "Weeks" w ON m."WeekId" = w."Id" 
    JOIN "Players" pa ON m."PlayerAId" = pa."Id" 
    JOIN "Players" pb ON m."PlayerBId" = pb."Id" 
    ORDER BY w."WeekNumber"
    '''
    
    cursor.execute(query)
    matchups = cursor.fetchall()
    
    # Get player names
    cursor.execute('SELECT "Id", "FirstName" || \' \' || "LastName" as FullName FROM "Players"')
    players = dict(cursor.fetchall())
    
    cursor.close()
    conn.close()
    
    # Track all pairings
    duplicate_tracking = defaultdict(list)
    
    for week, player_a, player_b, player_a_id, player_b_id in matchups:
        # Normalize pairing (always put smaller ID first to avoid duplicates)
        pair = tuple(sorted([player_a_id, player_b_id]))
        duplicate_tracking[pair].append(week)
    
    # Find duplicates
    duplicates = [(pair, weeks) for pair, weeks in duplicate_tracking.items() if len(weeks) > 1]
    
    print("🔍 Duplicate pairings found:")
    for pair, weeks in duplicates:
        player_a_name = players[pair[0]]
        player_b_name = players[pair[1]]
        print(f"  {player_a_name} vs {player_b_name} - appears in weeks: {weeks}")

if __name__ == "__main__":
    find_duplicates()
