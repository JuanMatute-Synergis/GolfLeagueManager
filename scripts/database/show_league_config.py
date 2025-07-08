#!/usr/bin/env python3
"""
Show current league configuration for a tenant.
Usage: python3 show_league_config.py <tenant_name>
"""

import sys
import psycopg2

# Database connection settings
DB_CONFIG = {
    'host': 'localhost',
    'port': 5432,
    'user': 'golfuser',
    'password': 'golfpassword'
}

def print_usage():
    print("Usage: python3 show_league_config.py <tenant_name>")
    print("")
    print("Examples:")
    print("  python3 show_league_config.py southmoore")

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

def show_league_config(cursor):
    """Show current league configuration."""
    
    # Get league settings with season info
    cursor.execute('''
        SELECT ls."CoursePar", ls."CourseRating", ls."SlopeRating", 
               ls."MaxRoundsForHandicap", ls."HoleWinPoints", ls."MatchWinBonus",
               s."Name" as "SeasonName", s."Year", s."StartDate", s."EndDate"
        FROM "LeagueSettings" ls
        JOIN "Seasons" s ON ls."SeasonId" = s."Id"
        LIMIT 1
    ''')
    
    result = cursor.fetchone()
    if not result:
        print("❌ No league settings found")
        return False
    
    par, rating, slope, max_rounds, hole_points, match_bonus, season_name, year, start_date, end_date = result
    
    print(f"🏌️  League Configuration")
    print("=" * 50)
    print(f"Season: {season_name} ({year})")
    print(f"Period: {start_date.strftime('%B %d, %Y')} - {end_date.strftime('%B %d, %Y')}")
    print("")
    print("🏌️  Course Settings:")
    print(f"   Par: {par}")
    print(f"   Course Rating: {rating}")
    print(f"   Slope Rating: {slope}")
    print("")
    print("📊 Scoring Settings:")
    print(f"   Hole Win Points: {hole_points}")
    print(f"   Match Win Bonus: {match_bonus}")
    print(f"   Max Rounds for Handicap: {max_rounds}")
    print("")
    
    # Check if we can match this to a specific course
    cursor.execute('''
        SELECT "Name", "Location", "TotalYardage"
        FROM "Courses" 
        WHERE "TotalPar" = %s AND "CourseRating" = %s AND "SlopeRating" = %s
    ''', (par, rating, slope))
    
    course_result = cursor.fetchone()
    if course_result:
        course_name, location, yardage = course_result
        print("🏌️  Matching Course:")
        print(f"   Name: {course_name}")
        print(f"   Location: {location}")
        print(f"   Yardage: {yardage} yards")
        print("")
    
    # Show player count
    cursor.execute('SELECT COUNT(*) FROM "Players"')
    player_count = cursor.fetchone()[0]
    
    # Show week count
    cursor.execute('SELECT COUNT(*) FROM "Weeks"')
    week_count = cursor.fetchone()[0]
    
    # Show matchup count
    cursor.execute('SELECT COUNT(*) FROM "Matchups"')
    matchup_count = cursor.fetchone()[0]
    
    print("📈 League Statistics:")
    print(f"   Players: {player_count}")
    print(f"   Weeks: {week_count}")
    print(f"   Matchups: {matchup_count}")
    
    return True

def main():
    if len(sys.argv) != 2:
        print_usage()
        return 1
    
    tenant_name = sys.argv[1]
    
    print(f"📊 Golf League Manager - League Configuration")
    print(f"🏢 Tenant: {tenant_name}")
    print("")
    
    # Connect to database
    conn = get_database_connection(tenant_name)
    if not conn:
        return 1
    
    try:
        cursor = conn.cursor()
        
        if not show_league_config(cursor):
            return 1
        
        return 0
        
    except Exception as e:
        print(f"❌ Error: {e}")
        return 1
    finally:
        conn.close()

if __name__ == "__main__":
    sys.exit(main())
