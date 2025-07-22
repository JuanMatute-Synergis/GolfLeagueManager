#!/usr/bin/env python3
"""
Set Southmoore Golf Course as the default course for a tenant's league settings.
Usage: python3 set_default_course.py <tenant_name>
"""

import sys
import psycopg2
from datetime import datetime, timezone

# Database connection settings
DB_CONFIG = {
    'host': 'localhost',
    'port': 5432,
    'user': 'golfuser',
    'password': 'golfpassword'
}

def print_usage():
    print("Usage: python3 set_default_course.py <tenant_name>")
    print("")
    print("Examples:")
    print("  python3 set_default_course.py southmoore")

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

def get_course_details(cursor, course_name="Southmoore Golf Course"):
    """Get course details by name."""
    cursor.execute('''
        SELECT "Id", "Name", "TotalPar", "CourseRating", "SlopeRating" 
        FROM "Courses" 
        WHERE "Name" = %s
    ''', (course_name,))
    
    result = cursor.fetchone()
    if result:
        return {
            'id': result[0],
            'name': result[1],
            'total_par': result[2],
            'course_rating': result[3],
            'slope_rating': result[4]
        }
    return None

def update_league_settings(cursor, course_details):
    """Update league settings to use the specified course as default."""
    
    print(f"🏌️  Setting '{course_details['name']}' as default course...")
    print(f"   📊 Par: {course_details['total_par']}")
    print(f"   📊 Course Rating: {course_details['course_rating']}")
    print(f"   📊 Slope Rating: {course_details['slope_rating']}")
    
    # Update existing league settings
    cursor.execute('''
        UPDATE "LeagueSettings" 
        SET "CoursePar" = %s,
            "CourseRating" = %s,
            "SlopeRating" = %s,
            "ModifiedDate" = %s
        WHERE "Id" IN (SELECT "Id" FROM "LeagueSettings" LIMIT 1)
    ''', (
        course_details['total_par'],
        course_details['course_rating'],
        course_details['slope_rating'],
        datetime.now(timezone.utc)
    ))
    
    rows_updated = cursor.rowcount
    
    if rows_updated > 0:
        print(f"✅ Updated {rows_updated} league settings record(s)")
    else:
        print("⚠️  No existing league settings found to update")
    
    return rows_updated

def verify_settings(cursor):
    """Verify the updated league settings."""
    cursor.execute('''
        SELECT ls."CoursePar", ls."CourseRating", ls."SlopeRating", ls."ModifiedDate",
               s."Name" as "SeasonName"
        FROM "LeagueSettings" ls
        JOIN "Seasons" s ON ls."SeasonId" = s."Id"
        LIMIT 1
    ''')
    
    result = cursor.fetchone()
    if result:
        par, rating, slope, modified_date, season_name = result
        print("")
        print("📊 Current League Settings:")
        print(f"   Season: {season_name}")
        print(f"   Course Par: {par}")
        print(f"   Course Rating: {rating}")
        print(f"   Slope Rating: {slope}")
        print(f"   Last Modified: {modified_date}")
        return True
    return False

def main():
    if len(sys.argv) != 2:
        print_usage()
        return 1
    
    tenant_name = sys.argv[1]
    
    print(f"🏌️  Golf League Manager - Set Default Course")
    print(f"📊 Tenant: {tenant_name}")
    print("")
    
    # Connect to database
    print("🔌 Connecting to database...")
    conn = get_database_connection(tenant_name)
    if not conn:
        return 1
    
    try:
        cursor = conn.cursor()
        
        # Get course details
        print("🔍 Looking for Southmoore Golf Course...")
        course_details = get_course_details(cursor)
        
        if not course_details:
            print("❌ Southmoore Golf Course not found in database")
            print("💡 Please import the course first using import_southmoore_course.py")
            return 1
        
        print(f"✅ Found course: {course_details['name']}")
        print("")
        
        # Update league settings
        rows_updated = update_league_settings(cursor, course_details)
        
        if rows_updated == 0:
            print("❌ No league settings were updated")
            return 1
        
        # Verify the update
        if not verify_settings(cursor):
            print("❌ Failed to verify updated settings")
            return 1
        
        # Commit changes
        conn.commit()
        
        print("")
        print("✅ Southmoore Golf Course is now set as the default course for this league!")
        print("")
        print("📋 What this means:")
        print("   - All handicap calculations will use Southmoore's course rating and slope")
        print("   - League scoring will be based on Southmoore's par 71 layout")
        print("   - Players' scores will be adjusted according to Southmoore's difficulty")
        
        return 0
        
    except Exception as e:
        print(f"❌ Error during update: {e}")
        conn.rollback()
        return 1
    finally:
        conn.close()

if __name__ == "__main__":
    sys.exit(main())
