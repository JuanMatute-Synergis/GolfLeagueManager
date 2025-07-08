#!/usr/bin/env python3
"""
Import Southmoore Golf Course data into Golf League Manager database.
Usage: python3 import_southmoore_course.py <tenant_name>
"""

import sys
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
    print("Usage: python3 import_southmoore_course.py <tenant_name>")
    print("")
    print("Examples:")
    print("  python3 import_southmoore_course.py southmoore")

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

def create_course(cursor):
    """Create Southmoore Golf Course."""
    
    # Course data from the website
    course_data = {
        'name': 'Southmoore Golf Course',
        'location': '235 Moorestown Dr, Bath, Pennsylvania 18014',
        'total_par': 71,
        'total_yardage': 5746,  # White tees yardage
        'slope_rating': 128,
        'course_rating': 70.4
    }
    
    course_id = str(uuid4())
    
    print(f"🏌️  Creating course: {course_data['name']}")
    
    cursor.execute('''
        INSERT INTO "Courses" (
            "Id", "Name", "Location", "TotalPar", 
            "TotalYardage", "SlopeRating", "CourseRating"
        ) VALUES (
            %s, %s, %s, %s, %s, %s, %s
        )
    ''', (
        course_id, course_data['name'], course_data['location'], 
        course_data['total_par'], course_data['total_yardage'], 
        course_data['slope_rating'], course_data['course_rating']
    ))
    
    print(f"✅ Course created successfully")
    return course_id

def create_course_holes(cursor, course_id):
    """Create the 18 holes for Southmoore Golf Course."""
    
    # Hole data from the scorecard (using White tees as default)
    holes_data = [
        # Front 9
        {'hole_number': 1, 'par': 4, 'yardage': 381, 'handicap': 3},
        {'hole_number': 2, 'par': 4, 'yardage': 354, 'handicap': 5},
        {'hole_number': 3, 'par': 3, 'yardage': 104, 'handicap': 17},  # The waterfall hole
        {'hole_number': 4, 'par': 5, 'yardage': 452, 'handicap': 7},
        {'hole_number': 5, 'par': 3, 'yardage': 154, 'handicap': 13},
        {'hole_number': 6, 'par': 5, 'yardage': 469, 'handicap': 1},
        {'hole_number': 7, 'par': 4, 'yardage': 320, 'handicap': 15},
        {'hole_number': 8, 'par': 4, 'yardage': 352, 'handicap': 9},
        {'hole_number': 9, 'par': 4, 'yardage': 364, 'handicap': 11},
        
        # Back 9
        {'hole_number': 10, 'par': 4, 'yardage': 373, 'handicap': 4},
        {'hole_number': 11, 'par': 4, 'yardage': 274, 'handicap': 14},
        {'hole_number': 12, 'par': 3, 'yardage': 135, 'handicap': 18},
        {'hole_number': 13, 'par': 4, 'yardage': 264, 'handicap': 16},
        {'hole_number': 14, 'par': 5, 'yardage': 478, 'handicap': 6},
        {'hole_number': 15, 'par': 4, 'yardage': 413, 'handicap': 2},
        {'hole_number': 16, 'par': 3, 'yardage': 173, 'handicap': 12},
        {'hole_number': 17, 'par': 4, 'yardage': 335, 'handicap': 10},
        {'hole_number': 18, 'par': 4, 'yardage': 351, 'handicap': 8}
    ]
    
    print(f"🏌️  Creating {len(holes_data)} holes...")
    
    for hole in holes_data:
        hole_id = str(uuid4())
        
        cursor.execute('''
            INSERT INTO "CourseHoles" (
                "Id", "CourseId", "HoleNumber", "Par", "Yardage", "HandicapIndex"
            ) VALUES (
                %s, %s, %s, %s, %s, %s
            )
        ''', (
            hole_id, course_id, hole['hole_number'], 
            hole['par'], hole['yardage'], hole['handicap']
        ))
    
    print(f"✅ All {len(holes_data)} holes created successfully")

def main():
    if len(sys.argv) != 2:
        print_usage()
        return 1
    
    tenant_name = sys.argv[1]
    
    print(f"🏌️  Golf League Manager - Southmoore Course Import")
    print(f"📊 Tenant: {tenant_name}")
    print("")
    
    # Connect to database
    print("🔌 Connecting to database...")
    conn = get_database_connection(tenant_name)
    if not conn:
        return 1
    
    try:
        cursor = conn.cursor()
        
        # Check if course already exists
        cursor.execute('SELECT COUNT(*) FROM "Courses" WHERE "Name" = %s', ('Southmoore Golf Course',))
        if cursor.fetchone()[0] > 0:
            print("⚠️  Southmoore Golf Course already exists in the database")
            print("🗑️  Removing existing course and holes...")
            
            # Get existing course ID
            cursor.execute('SELECT "Id" FROM "Courses" WHERE "Name" = %s', ('Southmoore Golf Course',))
            existing_course_id = cursor.fetchone()[0]
            
            # Delete existing holes
            cursor.execute('DELETE FROM "CourseHoles" WHERE "CourseId" = %s', (existing_course_id,))
            
            # Delete existing course
            cursor.execute('DELETE FROM "Courses" WHERE "Id" = %s', (existing_course_id,))
            
            print("✅ Existing course and holes removed")
            print("")
        
        # Create the course
        course_id = create_course(cursor)
        
        # Create the holes
        create_course_holes(cursor, course_id)
        
        # Commit changes
        conn.commit()
        
        print("")
        print("✅ Southmoore Golf Course import completed successfully!")
        print("")
        print("📊 Course Summary:")
        print("   - Name: Southmoore Golf Course")
        print("   - Location: Bath, Pennsylvania")
        print("   - Architect: Jim Blaukovitch (1994)")
        print("   - Type: Public Course")
        print("   - Holes: 18")
        print("   - Par: 71")
        print("   - Yardage: 5,746 yards (White tees)")
        print("   - Rating: 70.4")
        print("   - Slope: 128")
        print("   - Special Features: Waterfall on hole #3")
        
        return 0
        
    except Exception as e:
        print(f"❌ Error during import: {e}")
        conn.rollback()
        return 1
    finally:
        conn.close()

if __name__ == "__main__":
    sys.exit(main())
