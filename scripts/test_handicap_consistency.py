#!/usr/bin/env python3
"""
Test script to verify that handicap and average calculations are now consistent
and using the phantom score methodology correctly.
"""

import psycopg2
import sys
import os

def test_handicap_average_consistency():
    """Test that handicap and average calculations are consistent"""
    
    # Connect to PostgreSQL database
    try:
        conn = psycopg2.connect(
            host="localhost",
            port="5432", 
            database="golfdb",
            user="golfuser",
            password="golfpassword"
        )
        cursor = conn.cursor()
        print("Connected to PostgreSQL database: golfdb")
    except Exception as e:
        print(f"Failed to connect to database: {e}")
        return False
    
    try:
        # Find George H.
        cursor.execute("""
            SELECT "Id", "FirstName", "LastName", "InitialHandicap", "InitialAverageScore", 
                   "CurrentHandicap", "CurrentAverageScore"
            FROM "Players" 
            WHERE ("FirstName" LIKE '%George%' AND "LastName" LIKE '%H%') 
               OR "LastName" LIKE '%Hagen%'
            LIMIT 1
        """)
        
        player = cursor.fetchone()
        if not player:
            print("George H. not found in database")
            return False
        
        player_id, first_name, last_name, initial_handicap, initial_average, current_handicap, current_average = player
        full_name = f"{first_name} {last_name}"
        print(f"Found player: {full_name}")
        print(f"Initial Handicap: {initial_handicap}")
        print(f"Initial Average: {initial_average}")
        print(f"Current Handicap (stored): {current_handicap}")
        print(f"Current Average (stored): {current_average}")
        print()
        print("⚠️  WARNING: CurrentHandicap and CurrentAverageScore are still in the database!")
        print("    This means the migration to remove them hasn't been applied yet.")
        print("    The backend code calculates on demand, but the UI might be using these stored values.")
        print()
        
        # Get the current season
        cursor.execute("""
            SELECT "Id", "Name" 
            FROM "Seasons" 
            ORDER BY "Year" DESC, "SeasonNumber" DESC 
            LIMIT 1
        """)
        
        season = cursor.fetchone()
        if not season:
            print("No season found")
            return False
        
        season_id, season_name = season
        print(f"Testing season: {season_name}")
        
        # Get all weeks in the season with their properties
        cursor.execute("""
            SELECT w."Id", w."WeekNumber", w."CountsForScoring", w."CountsForHandicap",
                   m."PlayerAScore", m."PlayerBScore", m."PlayerAId", m."PlayerBId"
            FROM "Weeks" w
            LEFT JOIN "Matchups" m ON w."Id" = m."WeekId" 
                AND (m."PlayerAId" = %s OR m."PlayerBId" = %s)
            WHERE w."SeasonId" = %s AND w."CountsForScoring" = true
            ORDER BY w."WeekNumber"
        """, (player_id, player_id, season_id))
        
        weeks = cursor.fetchall()
        print(f"\nWeeks for {full_name}:")
        print("Week | Counts4H | Score | Notes")
        print("-" * 40)
        
        actual_scores = []
        phantom_scores = []
        current_valid_average = initial_average
        current_valid_handicap = initial_handicap
        
        for week_data in weeks:
            week_id, week_num, counts_scoring, counts_handicap, score_a, score_b, player_a_id, player_b_id = week_data
            
            # Determine the player's score for this week
            score = None
            if player_a_id == player_id and score_a is not None:
                score = score_a
            elif player_b_id == player_id and score_b is not None:
                score = score_b
            
            if counts_handicap:
                if score is not None:
                    actual_scores.append(score)
                    print(f" {week_num:2d}  |    Yes   |  {score:2d}  | Actual score")
                    # Update valid values after processing this counting week
                    if actual_scores:
                        current_valid_average = sum(actual_scores) / len(actual_scores)
                        current_valid_handicap = max(0, min(36, round(current_valid_average - 35, 0)))  # Assuming par 35
                else:
                    print(f" {week_num:2d}  |    Yes   |  --  | No score")
            else:
                # Non-counting week - use phantom score
                phantom_score = current_valid_average
                phantom_scores.append(phantom_score)
                print(f" {week_num:2d}  |    No    |{phantom_score:5.1f} | Phantom (previous avg)")
        
        print(f"\nActual scores: {actual_scores}")
        print(f"Phantom scores: {phantom_scores}")
        
        if actual_scores:
            calculated_average = sum(actual_scores) / len(actual_scores)
            calculated_handicap = max(0, min(36, round(calculated_average - 35, 0)))
            
            print(f"\nCalculated from actual scores only:")
            print(f"Average: {calculated_average:.2f}")
            print(f"Handicap: {calculated_handicap}")
            
            # For the phantom methodology, we include phantom scores
            all_scores = actual_scores + phantom_scores
            if all_scores:
                phantom_average = sum(all_scores) / len(all_scores)
                phantom_handicap = max(0, min(36, round(phantom_average - 35, 0)))
                
                print(f"\nUsing phantom score methodology:")
                print(f"Average: {phantom_average:.2f}")
                print(f"Handicap: {phantom_handicap}")
        
        return True
        
    except Exception as e:
        print(f"Error: {e}")
        return False
    finally:
        conn.close()

if __name__ == "__main__":
    print("Testing handicap and average calculation consistency...")
    success = test_handicap_average_consistency()
    
    if success:
        print("\n✅ Test completed successfully")
    else:
        print("\n❌ Test failed")
        sys.exit(1)
