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

def quick_round_robin_check():
    """Quick check to verify we still have a valid round-robin"""
    conn = psycopg2.connect(**conn_params)
    cursor = conn.cursor()
    
    query = '''
    SELECT 
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
    
    cursor.close()
    conn.close()
    
    # Track all pairings
    pairings = set()
    duplicate_tracking = defaultdict(int)
    
    for player_a, player_b, player_a_id, player_b_id in matchups:
        # Normalize pairing (always put smaller ID first to avoid duplicates)
        pair = tuple(sorted([player_a_id, player_b_id]))
        pairings.add(pair)
        duplicate_tracking[pair] += 1
    
    # Check for duplicates
    duplicates = [(pair, count) for pair, count in duplicate_tracking.items() if count > 1]
    
    print(f"Total unique pairings: {len(pairings)}")
    print(f"Expected unique pairings: 45")
    
    if duplicates:
        print(f"\n❌ Found {len(duplicates)} duplicate pairings:")
        for pair, count in duplicates:
            print(f"  Pair appears {count} times")
        return False
    elif len(pairings) == 45:
        print("✅ Perfect round-robin maintained!")
        return True
    else:
        print(f"❌ Wrong number of pairings: {len(pairings)} (expected 45)")
        return False

if __name__ == "__main__":
    quick_round_robin_check()
