#!/usr/bin/env python3
"""
Test script to verify George H.'s handicap calculation using PostgreSQL database.
This script connects to the database and checks if the phantom score logic is working correctly.
"""

import psycopg2
import json
from datetime import datetime

# Database connection parameters
DB_CONFIG = {
    'host': 'localhost',
    'port': 5432,
    'database': 'golfdb',
    'user': 'golfuser',
    'password': 'golfpassword'
}

def connect_to_db():
    """Connect to PostgreSQL database"""
    try:
        conn = psycopg2.connect(**DB_CONFIG)
        return conn
    except Exception as e:
        print(f"Error connecting to database: {e}")
        return None

def find_george_h():
    """Find George H. in the database"""
    conn = connect_to_db()
    if not conn:
        return None
    
    try:
        cursor = conn.cursor()
        cursor.execute("""
            SELECT "Id", "FirstName", "LastName", "Phone", "InitialHandicap", "InitialAverageScore"
            FROM "Players" 
            WHERE "FirstName" LIKE 'George%' AND "LastName" LIKE 'H%'
            OR "Phone" LIKE '%428-4032%'
        """)
        
        players = cursor.fetchall()
        print("Found players matching George H.:")
        for player in players:
            print(f"  ID: {player[0]}")
            print(f"  Name: {player[1]} {player[2]}")
            print(f"  Phone: {player[3]}")
            print(f"  Initial Handicap: {player[4]}")
            print(f"  Initial Average: {player[5]}")
            print()
        
        return players[0] if players else None
        
    except Exception as e:
        print(f"Error finding George H.: {e}")
        return None
    finally:
        conn.close()

def get_player_scores(player_id):
    """Get all scores for a player"""
    conn = connect_to_db()
    if not conn:
        return []
    
    try:
        cursor = conn.cursor()
        cursor.execute("""
            SELECT 
                w."WeekNumber",
                w."CountsForHandicap",
                w."CountsForScoring",
                CASE 
                    WHEN m."PlayerAId" = %s THEN m."PlayerAScore"
                    ELSE m."PlayerBScore"
                END as score,
                CASE 
                    WHEN m."PlayerAId" = %s THEN m."PlayerAAbsent"
                    ELSE m."PlayerBAbsent"
                END as absent,
                s."Name" as season_name
            FROM "Matchups" m
            JOIN "Weeks" w ON m."WeekId" = w."Id"
            JOIN "Seasons" s ON w."SeasonId" = s."Id"
            WHERE (m."PlayerAId" = %s OR m."PlayerBId" = %s)
            AND ((m."PlayerAId" = %s AND m."PlayerAScore" IS NOT NULL AND NOT m."PlayerAAbsent")
                 OR (m."PlayerBId" = %s AND m."PlayerBScore" IS NOT NULL AND NOT m."PlayerBAbsent"))
            ORDER BY w."WeekNumber"
        """, (player_id, player_id, player_id, player_id, player_id, player_id))
        
        scores = cursor.fetchall()
        print(f"Found {len(scores)} scores for player:")
        for score in scores:
            week_num, counts_handicap, counts_scoring, score_val, absent, season = score
            counting_status = "Counts" if counts_handicap else "Non-counting"
            print(f"  Week {week_num}: Score {score_val} ({counting_status})")
        
        return scores
        
    except Exception as e:
        print(f"Error getting player scores: {e}")
        return []
    finally:
        conn.close()

def get_league_settings():
    """Get league settings to understand calculation method"""
    conn = connect_to_db()
    if not conn:
        return None
    
    try:
        cursor = conn.cursor()
        cursor.execute("""
            SELECT "CoursePar", "CourseRating", "SlopeRating", "HandicapMethod"
            FROM "LeagueSettings"
            ORDER BY "CreatedDate" DESC
            LIMIT 1
        """)
        
        settings = cursor.fetchone()
        if settings:
            print("League Settings:")
            print(f"  Course Par: {settings[0]}")
            print(f"  Course Rating: {settings[1]}")
            print(f"  Slope Rating: {settings[2]}")
            print(f"  Handicap Method: {settings[3]}")
            print()
        
        return settings
        
    except Exception as e:
        print(f"Error getting league settings: {e}")
        return None
    finally:
        conn.close()

def simulate_phantom_score_calculation(player_data, scores, league_settings):
    """Simulate the phantom score calculation logic"""
    if not league_settings:
        print("No league settings found!")
        return None
    
    initial_handicap = player_data[4]  # InitialHandicap
    course_par = league_settings[0]
    
    print(f"Simulating handicap calculation for {player_data[1]} {player_data[2]}:")
    print(f"Initial Handicap: {initial_handicap}")
    print(f"Course Par: {course_par}")
    print()
    
    scores_for_calculation = []
    current_valid_handicap = initial_handicap
    
    print("Week-by-week calculation:")
    for i, score_data in enumerate(scores):
        week_num, counts_handicap, counts_scoring, score_val, absent, season = score_data
        
        if counts_handicap:
            # Actual score week
            scores_for_calculation.append(score_val)
            print(f"Week {week_num}: Actual score {score_val} (added to calculation)")
            
            # Update valid handicap after adding this score
            if scores_for_calculation:
                avg_score = sum(scores_for_calculation) / len(scores_for_calculation)
                current_valid_handicap = round(avg_score - course_par)
                current_valid_handicap = max(0, min(36, current_valid_handicap))
                print(f"  -> Updated valid handicap: {current_valid_handicap}")
        else:
            # Non-counting week - use phantom score
            phantom_score = current_valid_handicap + course_par
            scores_for_calculation.append(phantom_score)
            print(f"Week {week_num}: Non-counting week, using phantom score {phantom_score} (based on handicap {current_valid_handicap})")
    
    if scores_for_calculation:
        final_avg = sum(scores_for_calculation) / len(scores_for_calculation)
        final_handicap = max(0, min(36, round(final_avg - course_par)))
        
        print()
        print("Final calculation:")
        print(f"All scores: {scores_for_calculation}")
        print(f"Average of all scores: {final_avg:.2f}")
        print(f"Final handicap: {final_handicap} (avg {final_avg:.2f} - par {course_par})")
        
        return final_handicap, final_avg
    
    return initial_handicap, player_data[5]  # Return initial values if no scores

def main():
    print("=== George H. Handicap Verification Test ===")
    print("Testing the phantom score calculation logic")
    print()
    
    # Find George H.
    george = find_george_h()
    if not george:
        print("George H. not found in database!")
        return
    
    print(f"Testing calculations for: {george[1]} {george[2]}")
    print()
    
    # Get league settings
    settings = get_league_settings()
    if not settings:
        print("League settings not found!")
        return
    
    # Get player scores
    scores = get_player_scores(george[0])
    if not scores:
        print("No scores found for George H.!")
        return
    
    print()
    
    # Simulate the calculation
    final_handicap, final_avg = simulate_phantom_score_calculation(george, scores, settings)
    
    print()
    print("=== Expected Results ===")
    print(f"Handicap should be: {final_handicap}")
    print(f"Average should be: {final_avg:.2f}")
    print()
    print("This should match what the UI and PDF reports show.")

if __name__ == "__main__":
    main()
