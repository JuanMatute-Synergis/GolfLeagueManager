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

def get_all_matchups():
    """Get all matchups from the database with player names and week numbers"""
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
    ORDER BY w."WeekNumber", pa."LastName", pb."LastName"
    '''
    
    cursor.execute(query)
    matchups = cursor.fetchall()
    
    cursor.close()
    conn.close()
    
    return matchups

def get_all_players():
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

def analyze_round_robin():
    """Analyze if we have a perfect round-robin schedule"""
    print("🔍 Analyzing Round-Robin Schedule")
    print("=" * 50)
    
    matchups = get_all_matchups()
    players = get_all_players()
    
    print(f"Total players: {len(players)}")
    print(f"Total matchups: {len(matchups)}")
    print()
    
    # Create player ID to name mapping
    player_names = {player_id: name for player_id, name in players}
    player_ids = [player_id for player_id, name in players]
    
    # Track all pairings
    pairings = set()
    week_matchups = defaultdict(list)
    
    for week, player_a, player_b, player_a_id, player_b_id in matchups:
        # Normalize pairing (always put smaller ID first to avoid duplicates)
        pair = tuple(sorted([player_a_id, player_b_id]))
        pairings.add(pair)
        week_matchups[week].append((player_a, player_b))
    
    # Print matchups by week
    for week in sorted(week_matchups.keys()):
        print(f"Week {week}:")
        for player_a, player_b in week_matchups[week]:
            print(f"  {player_a} vs {player_b}")
        print()
    
    # Calculate expected number of unique pairings
    n_players = len(players)
    expected_pairings = n_players * (n_players - 1) // 2
    actual_pairings = len(pairings)
    
    print(f"Expected unique pairings: {expected_pairings}")
    print(f"Actual unique pairings: {actual_pairings}")
    print()
    
    # Check for missing pairings
    all_possible_pairs = set()
    for i in range(len(player_ids)):
        for j in range(i + 1, len(player_ids)):
            pair = tuple(sorted([player_ids[i], player_ids[j]]))
            all_possible_pairs.add(pair)
    
    missing_pairs = all_possible_pairs - pairings
    duplicate_tracking = defaultdict(int)
    
    # Count occurrences of each pairing
    for week, player_a, player_b, player_a_id, player_b_id in matchups:
        pair = tuple(sorted([player_a_id, player_b_id]))
        duplicate_tracking[pair] += 1
    
    duplicates = [(pair, count) for pair, count in duplicate_tracking.items() if count > 1]
    
    print("🔍 Analysis Results:")
    print("-" * 30)
    
    if actual_pairings == expected_pairings and not missing_pairs and not duplicates:
        print("✅ PERFECT ROUND-ROBIN: Every player plays every other player exactly once!")
    else:
        print(f"❌ NOT A PERFECT ROUND-ROBIN")
        
        if missing_pairs:
            print(f"\n🚫 Missing pairings ({len(missing_pairs)}):")
            for pair in missing_pairs:
                player_a_name = player_names[pair[0]]
                player_b_name = player_names[pair[1]]
                print(f"  {player_a_name} vs {player_b_name}")
        
        if duplicates:
            print(f"\n🔄 Duplicate pairings ({len(duplicates)}):")
            for pair, count in duplicates:
                player_a_name = player_names[pair[0]]
                player_b_name = player_names[pair[1]]
                print(f"  {player_a_name} vs {player_b_name} (appears {count} times)")
    
    return actual_pairings == expected_pairings and not missing_pairs and not duplicates

def create_visual_matrix():
    """Create a visual matrix showing which players have played each other"""
    players = get_all_players()
    matchups = get_all_matchups()
    
    # Create player ID to index mapping
    player_ids = [player_id for player_id, name in players]
    player_names = [name for player_id, name in players]
    id_to_index = {player_id: i for i, player_id in enumerate(player_ids)}
    
    # Create matrix
    n = len(players)
    matrix = [[0 for _ in range(n)] for _ in range(n)]
    
    # Fill matrix with matchup data
    for week, player_a, player_b, player_a_id, player_b_id in matchups:
        i = id_to_index[player_a_id]
        j = id_to_index[player_b_id]
        matrix[i][j] += 1
        matrix[j][i] += 1  # Symmetric
    
    print("\n📊 Visual Matrix (showing how many times each pair has played):")
    print("=" * 60)
    
    # Print header
    print("Player".ljust(15), end="")
    for i, name in enumerate(player_names):
        print(f"{i+1:>3}", end="")
    print()
    
    # Print rows
    for i, name in enumerate(player_names):
        print(f"{i+1:>2}. {name[:12]:<12}", end="")
        for j in range(n):
            if i == j:
                print("  -", end="")
            else:
                print(f"{matrix[i][j]:>3}", end="")
        print()
    
    print("\nLegend:")
    print("- = Same player")
    print("0 = Never played")
    print("1 = Played once (perfect)")
    print("2+ = Played multiple times (duplicate)")

if __name__ == "__main__":
    try:
        is_perfect = analyze_round_robin()
        create_visual_matrix()
        
        if is_perfect:
            print("\n🎉 SUCCESS: The current schedule is a perfect round-robin!")
        else:
            print("\n⚠️  The current schedule needs to be fixed to achieve a perfect round-robin.")
            
    except Exception as e:
        print(f"❌ Error: {e}")
