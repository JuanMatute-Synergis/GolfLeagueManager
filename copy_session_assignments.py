#!/usr/bin/env python3
import psycopg2
import uuid
from datetime import datetime

def main():
    try:
        print('Connecting to database...')
        conn = psycopg2.connect(
            host='192.168.6.67',
            port=5432,
            database='golfdb_htlyons',
            user='golfuser',
            password='golfpassword'
        )
        print('Connected successfully!')
        
        cursor = conn.cursor()
        
        # Get the 2025 season ID
        cursor.execute('SELECT "Id" FROM "Seasons" WHERE "Name" = %s AND "Year" = %s', ('2025', 2025))
        result = cursor.fetchone()
        
        if not result:
            print('2025 season not found')
            return
            
        season_id = result[0]
        print(f'Season ID: {season_id}')
        
        # Get all Session 1 assignments
        cursor.execute('''
            SELECT 
                "PlayerId",
                "FlightId",
                "IsFlightLeader",
                "HandicapAtAssignment"
            FROM "PlayerFlightAssignments"
            WHERE "SeasonId" = %s AND "SessionStartWeekNumber" = 1
        ''', (season_id,))
        
        session1_assignments = cursor.fetchall()
        print(f'Found {len(session1_assignments)} assignments in Session 1')
        
        # Check if Session 2 already has assignments
        cursor.execute('''
            SELECT COUNT(*) 
            FROM "PlayerFlightAssignments"
            WHERE "SeasonId" = %s AND "SessionStartWeekNumber" = 8
        ''', (season_id,))
        
        session2_count = cursor.fetchone()[0]
        print(f'Session 2 currently has {session2_count} assignments')
        
        if session2_count > 0:
            response = input(f'Session 2 already has {session2_count} assignments. Delete them and copy from Session 1? (y/N): ')
            if response.lower() != 'y':
                print('Cancelled.')
                return
                
            # Delete existing Session 2 assignments
            cursor.execute('''
                DELETE FROM "PlayerFlightAssignments"
                WHERE "SeasonId" = %s AND "SessionStartWeekNumber" = 8
            ''', (season_id,))
            print(f'Deleted {session2_count} existing Session 2 assignments')
        
        # Copy Session 1 assignments to Session 2 (Week 8)
        print(f'Copying {len(session1_assignments)} assignments to Session 2...')
        
        for assignment in session1_assignments:
            player_id, flight_id, is_flight_leader, handicap = assignment
            new_id = str(uuid.uuid4())
            
            cursor.execute('''
                INSERT INTO "PlayerFlightAssignments" 
                ("Id", "PlayerId", "FlightId", "SeasonId", "SessionStartWeekNumber", 
                 "IsFlightLeader", "HandicapAtAssignment", "AssignmentDate")
                VALUES (%s, %s, %s, %s, %s, %s, %s, %s)
            ''', (
                new_id,
                player_id,
                flight_id,
                season_id,
                8,  # Session 2 starts at Week 8
                is_flight_leader,
                handicap,
                datetime.utcnow()
            ))
        
        # Commit the changes
        conn.commit()
        print(f'✅ Successfully copied {len(session1_assignments)} assignments to Session 2!')
        
        # Verify the copy
        cursor.execute('''
            SELECT COUNT(*) 
            FROM "PlayerFlightAssignments"
            WHERE "SeasonId" = %s AND "SessionStartWeekNumber" = 8
        ''', (season_id,))
        
        new_count = cursor.fetchone()[0]
        print(f'Session 2 now has {new_count} assignments')
        
        cursor.close()
        conn.close()
        
    except Exception as e:
        print(f'Error: {e}')
        import traceback
        traceback.print_exc()
        if 'conn' in locals():
            conn.rollback()

if __name__ == '__main__':
    main()
