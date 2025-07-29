#!/usr/bin/env python3
import psycopg2
import sys

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
        
        if result:
            season_id = result[0]
            print(f'Season ID: {season_id}')
            
            # Check assignments by session
            sessions = [(1, 1), (8, 2), (15, 3)]  # (week, session_number)
            
            print('\n=== FLIGHT ASSIGNMENTS BY SESSION ===')
            for week, session_num in sessions:
                cursor.execute('''
                    SELECT COUNT(*) 
                    FROM "PlayerFlightAssignments" 
                    WHERE "SeasonId" = %s AND "SessionStartWeekNumber" = %s
                ''', (season_id, week))
                
                count = cursor.fetchone()[0]
                print(f'Session {session_num} (Week {week}): {count} assignments')
            
            # Get detailed Session 1 assignments
            print(f'\n=== SESSION 1 DETAILED ASSIGNMENTS ===')
            cursor.execute('''
                SELECT 
                    p."FirstName", 
                    p."LastName", 
                    f."Name" as flight_name,
                    pfa."IsFlightLeader",
                    pfa."PlayerId",
                    pfa."FlightId"
                FROM "PlayerFlightAssignments" pfa
                JOIN "Players" p ON pfa."PlayerId" = p."Id"
                JOIN "Flights" f ON pfa."FlightId" = f."Id"
                WHERE pfa."SeasonId" = %s AND pfa."SessionStartWeekNumber" = 1
                ORDER BY f."Name", p."LastName", p."FirstName"
            ''', (season_id,))
            
            session1_assignments = cursor.fetchall()
            print(f'Found {len(session1_assignments)} assignments in Session 1:')
            
            current_flight = None
            for assignment in session1_assignments:
                flight_name = assignment[2]
                if flight_name != current_flight:
                    current_flight = flight_name
                    print(f'\n  {flight_name}:')
                
                leader_indicator = ' (Leader)' if assignment[3] else ''
                print(f'    - {assignment[0]} {assignment[1]}{leader_indicator}')
        
        else:
            print('2025 season not found')
        
        cursor.close()
        conn.close()
        
    except Exception as e:
        print(f'Error: {e}')
        import traceback
        traceback.print_exc()

if __name__ == '__main__':
    main()
