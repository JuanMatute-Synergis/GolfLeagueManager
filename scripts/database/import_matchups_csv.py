#!/usr/bin/env python3
"""
Import matchups from CSV file to Golf League Manager database.
Usage: python3 import_matchups_csv.py <tenant_name> <csv_file_path>
"""

import sys
import csv
import psycopg2
from uuid import uuid4
import os

# Database connection settings
DB_CONFIG = {
    'host': 'localhost',
    'port': 5432,
    'user': 'golfuser',
    'password': 'golfpassword'
}

def print_usage():
    print("Usage: python3 import_matchups_csv.py <tenant_name> <csv_file_path>")
    print("")
    print("Examples:")
    print("  python3 import_matchups_csv.py southmoore /path/to/matchups.csv")
    print("")
    print("CSV Format:")
    print("  Week,Player 1,Player 2")
    print("  1,John Doe,Jane Smith")
    print("  2,John Doe,Bob Johnson")

def get_database_connection(tenant_name):
    """Get database connection for the specified tenant."""
    db_name = f"golfdb_{tenant_name}"
    try:
        conn = psycopg2.connect(
            host=DB_CONFIG['host'],
            port=DB_CONFIG['port'],
            database=db_name,
            user=DB_CONFIG['user'],
            password=DB_CONFIG['password']
        )
        return conn
    except psycopg2.Error as e:
        print(f"❌ Error connecting to database '{db_name}': {e}")
        return None

def get_players_map(cursor):
    """Get a mapping of player names to IDs."""
    cursor.execute('SELECT "Id", "FirstName", "LastName" FROM "Players"')
    players = cursor.fetchall()
    
    player_map = {}
    for player_id, first_name, last_name in players:
        full_name = f"{first_name} {last_name}"
        player_map[full_name] = player_id
    
    return player_map

def get_weeks_map(cursor):
    """Get a mapping of week numbers to IDs."""
    cursor.execute('SELECT "Id", "WeekNumber" FROM "Weeks" ORDER BY "WeekNumber"')
    weeks = cursor.fetchall()
    
    week_map = {}
    for week_id, week_number in weeks:
        week_map[week_number] = week_id
    
    return week_map

def parse_csv_file(csv_file_path):
    """Parse the CSV file and return matchup data."""
    matchups = []
    
    try:
        with open(csv_file_path, 'r', newline='', encoding='utf-8') as csvfile:
            reader = csv.DictReader(csvfile)
            
            # Check if required columns exist
            required_columns = ['Week', 'Player 1', 'Player 2']
            if not all(col in reader.fieldnames for col in required_columns):
                print(f"❌ Error: CSV file must contain columns: {', '.join(required_columns)}")
                return None
            
            for row_num, row in enumerate(reader, start=2):  # Start at 2 because of header
                try:
                    week_number = int(row['Week'])
                    player1_name = row['Player 1'].strip()
                    player2_name = row['Player 2'].strip()
                    
                    if not player1_name or not player2_name:
                        print(f"⚠️  Warning: Empty player name in row {row_num}, skipping")
                        continue
                    
                    matchups.append({
                        'week': week_number,
                        'player1': player1_name,
                        'player2': player2_name
                    })
                    
                except ValueError as e:
                    print(f"⚠️  Warning: Invalid data in row {row_num}: {e}")
                    continue
                    
    except FileNotFoundError:
        print(f"❌ Error: CSV file not found: {csv_file_path}")
        return None
    except Exception as e:
        print(f"❌ Error reading CSV file: {e}")
        return None
    
    return matchups

def clear_existing_matchups(cursor):
    """Clear existing matchups from the database."""
    cursor.execute('DELETE FROM "Matchups"')
    deleted_count = cursor.rowcount
    print(f"🗑️  Cleared {deleted_count} existing matchups")

def import_matchups(cursor, matchups, player_map, week_map):
    """Import matchups into the database."""
    imported_count = 0
    errors = []
    
    for matchup in matchups:
        week_number = matchup['week']
        player1_name = matchup['player1']
        player2_name = matchup['player2']
        
        # Check if week exists
        if week_number not in week_map:
            errors.append(f"Week {week_number} not found in database")
            continue
        
        # Check if players exist
        if player1_name not in player_map:
            errors.append(f"Player '{player1_name}' not found in database")
            continue
        
        if player2_name not in player_map:
            errors.append(f"Player '{player2_name}' not found in database")
            continue
        
        # Insert matchup
        week_id = week_map[week_number]
        player1_id = player_map[player1_name]
        player2_id = player_map[player2_name]
        
        try:
            cursor.execute('''
                INSERT INTO "Matchups" (
                    "Id", "WeekId", "PlayerAId", "PlayerBId", 
                    "PlayerAHolePoints", "PlayerBHolePoints", 
                    "PlayerAMatchWin", "PlayerBMatchWin",
                    "PlayerAAbsent", "PlayerAAbsentWithNotice",
                    "PlayerBAbsent", "PlayerBAbsentWithNotice"
                ) VALUES (
                    %s, %s, %s, %s, 
                    0, 0, 
                    false, false,
                    false, false,
                    false, false
                )
            ''', (str(uuid4()), week_id, player1_id, player2_id))
            
            imported_count += 1
            
        except psycopg2.Error as e:
            errors.append(f"Error inserting matchup Week {week_number}: {player1_name} vs {player2_name} - {e}")
    
    return imported_count, errors

def main():
    if len(sys.argv) != 3:
        print_usage()
        return 1
    
    tenant_name = sys.argv[1]
    csv_file_path = sys.argv[2]
    
    print(f"🏌️  Golf League Manager - Matchup Import")
    print(f"📊 Tenant: {tenant_name}")
    print(f"📁 CSV File: {csv_file_path}")
    print("")
    
    # Parse CSV file
    print("📖 Parsing CSV file...")
    matchups = parse_csv_file(csv_file_path)
    if not matchups:
        return 1
    
    print(f"✅ Found {len(matchups)} matchups in CSV file")
    print("")
    
    # Connect to database
    print("🔌 Connecting to database...")
    conn = get_database_connection(tenant_name)
    if not conn:
        return 1
    
    try:
        cursor = conn.cursor()
        
        # Get players and weeks
        print("👥 Loading players...")
        player_map = get_players_map(cursor)
        print(f"✅ Found {len(player_map)} players in database")
        
        print("📅 Loading weeks...")
        week_map = get_weeks_map(cursor)
        print(f"✅ Found {len(week_map)} weeks in database")
        print("")
        
        # Clear existing matchups
        print("🧹 Clearing existing matchups...")
        clear_existing_matchups(cursor)
        print("")
        
        # Import matchups
        print("⬆️  Importing matchups...")
        imported_count, errors = import_matchups(cursor, matchups, player_map, week_map)
        
        if errors:
            print(f"⚠️  {len(errors)} errors encountered:")
            for error in errors:
                print(f"   - {error}")
            print("")
        
        # Commit changes
        conn.commit()
        print(f"✅ Successfully imported {imported_count} matchups!")
        
        # Show summary
        print("")
        print("📊 Import Summary:")
        print(f"   - Total matchups in CSV: {len(matchups)}")
        print(f"   - Successfully imported: {imported_count}")
        print(f"   - Errors: {len(errors)}")
        
        return 0
        
    except Exception as e:
        print(f"❌ Error during import: {e}")
        conn.rollback()
        return 1
    finally:
        conn.close()

if __name__ == "__main__":
    sys.exit(main())
