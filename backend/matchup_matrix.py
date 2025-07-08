#!/usr/bin/env python3
"""
Create a visual matrix to show all golf league matchups across weeks 1-9
"""

import psycopg2

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

def create_matchup_matrix():
    """Create a visual matrix showing all matchups"""
    conn = get_database_connection()
    cur = conn.cursor()
    
    # Get all players
    cur.execute('SELECT "Id", "FirstName", "LastName" FROM "Players" ORDER BY "LastName"')
    players = cur.fetchall()
    player_names = [f"{row[1]} {row[2]}" for row in players]
    player_ids = [row[0] for row in players]
    
    print("GOLF LEAGUE MATCHUP MATRIX")
    print("=" * 80)
    print(f"Total Players: {len(players)}")
    print()
    
    # Get all matchups for weeks 1-9
    cur.execute('''
        SELECT 
            w."WeekNumber",
            pa."FirstName" || ' ' || pa."LastName" as player_a,
            pb."FirstName" || ' ' || pb."LastName" as player_b
        FROM "Matchups" m
        JOIN "Weeks" w ON m."WeekId" = w."Id"
        JOIN "Players" pa ON m."PlayerAId" = pa."Id"
        JOIN "Players" pb ON m."PlayerBId" = pb."Id"
        WHERE w."WeekNumber" <= 9
        ORDER BY w."WeekNumber", pa."LastName"
    ''')
    
    matchups = cur.fetchall()
    
    # Create a matrix to track who plays whom and in which week
    matchup_matrix = {}
    week_schedule = {}
    
    for week_num, player_a, player_b in matchups:
        # Normalize the pairing (alphabetical order)
        pair = tuple(sorted([player_a, player_b]))
        
        if pair not in matchup_matrix:
            matchup_matrix[pair] = []
        matchup_matrix[pair].append(week_num)
        
        # Track weekly schedule
        if week_num not in week_schedule:
            week_schedule[week_num] = []
        week_schedule[week_num].append((player_a, player_b))
    
    # Print week-by-week schedule
    print("WEEK-BY-WEEK SCHEDULE:")
    print("-" * 50)
    for week in sorted(week_schedule.keys()):
        print(f"Week {week}:")
        for player_a, player_b in week_schedule[week]:
            print(f"  {player_a} vs {player_b}")
        print()
    
    # Create a player vs player matrix
    print("PLAYER vs PLAYER MATRIX:")
    print("-" * 80)
    print("Shows week number(s) when players face each other")
    print("'X' means they never play each other")
    print()
    
    # Create matrix header
    short_names = [name.split()[0][0] + name.split()[1][0] for name in player_names]
    print("Player".ljust(15), end="")
    for short_name in short_names:
        print(short_name.ljust(4), end="")
    print()
    
    # Create matrix rows
    for i, player_a in enumerate(player_names):
        print(player_a.ljust(15), end="")
        for j, player_b in enumerate(player_names):
            if i == j:
                print("--".ljust(4), end="")  # Same player
            else:
                pair = tuple(sorted([player_a, player_b]))
                if pair in matchup_matrix:
                    weeks = ",".join(map(str, matchup_matrix[pair]))
                    print(weeks.ljust(4), end="")
                else:
                    print("X".ljust(4), end="")
        print()
    
    print()
    
    # Statistics
    total_possible_pairings = len(player_names) * (len(player_names) - 1) // 2
    actual_pairings = len(matchup_matrix)
    repeated_pairings = sum(1 for weeks in matchup_matrix.values() if len(weeks) > 1)
    
    print("STATISTICS:")
    print("-" * 30)
    print(f"Total possible unique pairings: {total_possible_pairings}")
    print(f"Actual pairings scheduled: {actual_pairings}")
    print(f"Coverage: {actual_pairings/total_possible_pairings*100:.1f}%")
    print(f"Repeated pairings: {repeated_pairings}")
    print()
    
    # Show missing pairings
    missing_pairings = []
    for i, player_a in enumerate(player_names):
        for j, player_b in enumerate(player_names[i+1:], i+1):
            pair = tuple(sorted([player_a, player_b]))
            if pair not in matchup_matrix:
                missing_pairings.append(pair)
    
    if missing_pairings:
        print("MISSING PAIRINGS (never play each other):")
        print("-" * 40)
        for player_a, player_b in missing_pairings:
            print(f"  {player_a} vs {player_b}")
        print()
    
    # Show repeated pairings
    repeated = [(pair, weeks) for pair, weeks in matchup_matrix.items() if len(weeks) > 1]
    if repeated:
        print("REPEATED PAIRINGS:")
        print("-" * 20)
        for (player_a, player_b), weeks in repeated:
            weeks_str = ", ".join(map(str, weeks))
            print(f"  {player_a} vs {player_b} (Weeks: {weeks_str})")
        print()
    
    # Game count per player
    player_game_count = {name: 0 for name in player_names}
    for week_num, player_a, player_b in matchups:
        player_game_count[player_a] += 1
        player_game_count[player_b] += 1
    
    print("GAMES PER PLAYER:")
    print("-" * 20)
    for player, count in sorted(player_game_count.items()):
        print(f"  {player}: {count} games")
    
    cur.close()
    conn.close()

if __name__ == "__main__":
    create_matchup_matrix()
