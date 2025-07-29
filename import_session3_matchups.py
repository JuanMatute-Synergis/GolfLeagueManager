#!/usr/bin/env python3
"""
Clear existing Session 3 matchups and import new ones from week15_matchups.py
"""

import requests
import json
from week15_matchups import generate_all_matchups

# Configuration
API_BASE_URL = "http://localhost:5274/api"
SEASON_ID = "a57df491-9860-4c01-a883-ab68e838adb7"  # 2025 season ID

def get_session3_weeks():
    """Get all weeks for Session 3 (weeks 15-21)"""
    try:
        response = requests.get(f"{API_BASE_URL}/weeks/season/{SEASON_ID}")
        response.raise_for_status()
        weeks = response.json()
        
        # Filter for Session 3 weeks (15-21)
        session3_weeks = [w for w in weeks if 15 <= w['weekNumber'] <= 21]
        return session3_weeks
    except requests.RequestException as e:
        print(f"Error fetching weeks: {e}")
        return []

def get_matchups_for_week(week_id):
    """Get all matchups for a specific week"""
    try:
        response = requests.get(f"{API_BASE_URL}/matchups/week/{week_id}")
        if response.status_code == 200:
            return response.json()
        return []
    except requests.RequestException as e:
        print(f"Error fetching matchups for week {week_id}: {e}")
        return []

def delete_matchup(matchup_id):
    """Delete a single matchup"""
    try:
        response = requests.delete(f"{API_BASE_URL}/matchups/{matchup_id}")
        return response.status_code == 200 or response.status_code == 204
    except requests.RequestException as e:
        print(f"Error deleting matchup {matchup_id}: {e}")
        return False

def clear_session3_matchups():
    """Clear all existing matchups for Session 3"""
    print("🧹 Clearing existing Session 3 matchups...")
    
    weeks = get_session3_weeks()
    if not weeks:
        print("❌ No Session 3 weeks found")
        return False
    
    total_deleted = 0
    for week in weeks:
        matchups = get_matchups_for_week(week['id'])
        print(f"  Week {week['weekNumber']}: Found {len(matchups)} matchups to delete")
        
        for matchup in matchups:
            if delete_matchup(matchup['id']):
                total_deleted += 1
            else:
                print(f"    ❌ Failed to delete matchup {matchup['id']}")
    
    print(f"✅ Deleted {total_deleted} existing matchups")
    return True

def get_all_players():
    """Get all players from the API"""
    try:
        response = requests.get(f"{API_BASE_URL}/players")
        response.raise_for_status()
        return response.json()
    except requests.RequestException as e:
        print(f"Error fetching players: {e}")
        return []

def get_flights_by_season():
    """Get all flights for the season"""
    try:
        response = requests.get(f"{API_BASE_URL}/flights/season/{SEASON_ID}")
        response.raise_for_status()
        return response.json()
    except requests.RequestException as e:
        print(f"Error fetching flights: {e}")
        return []

def find_player_by_name(players, full_name):
    """Find a player by their full name"""
    for player in players:
        player_full_name = f"{player['firstName']} {player['lastName']}"
        if player_full_name.strip().lower() == full_name.strip().lower():
            return player
    return None

def find_flight_by_name(flights, flight_name):
    """Find a flight by name (convert Flight X to X)"""
    # Convert "Flight 1" to "1"
    if flight_name.startswith("Flight "):
        flight_number = flight_name.replace("Flight ", "")
    else:
        flight_number = flight_name
        
    for flight in flights:
        if flight['name'] == flight_number:
            return flight
    return None

def find_week_by_number(weeks, week_number):
    """Find a week by its number"""
    for week in weeks:
        if week['weekNumber'] == week_number:
            return week
    return None

def create_matchup(week_id, player1_id, player2_id, flight_id=None):
    """Create a new matchup"""
    matchup_data = {
        "weekId": week_id,
        "playerAId": player1_id,
        "playerBId": player2_id
        # Note: Matchup model doesn't have flightId, flight info comes from player assignments
    }
    
    try:
        response = requests.post(f"{API_BASE_URL}/matchups", json=matchup_data)
        response.raise_for_status()
        return response.json()
    except requests.RequestException as e:
        print(f"Error creating matchup: {e}")
        return None

def import_new_matchups():
    """Import new matchups from week15_matchups.py"""
    print("📥 Importing new Session 3 matchups...")
    
    # Get required data
    weeks = get_session3_weeks()
    players = get_all_players()
    flights = get_flights_by_season()
    
    if not weeks or not players or not flights:
        print("❌ Failed to fetch required data")
        return False
    
    # Generate matchups from week15_matchups.py
    week_matchups = generate_all_matchups()
    
    successful_imports = 0
    failed_imports = 0
    
    for week_data in week_matchups:
        week_number = week_data['week']
        week_obj = find_week_by_number(weeks, week_number)
        
        if not week_obj:
            print(f"  ❌ Week {week_number} not found in database")
            continue
            
        print(f"  Week {week_number}: Importing matchups...")
        
        for flight_name, flight_matchups in week_data['flights'].items():
            flight_obj = find_flight_by_name(flights, flight_name)
            
            if not flight_obj:
                print(f"    ❌ Flight '{flight_name}' not found")
                continue
            
            for matchup in flight_matchups:
                player1_name = matchup['player1']['name']
                player2_name = matchup['player2']['name']
                
                # Handle corrected name
                if player1_name == "Danny Washburn":
                    player1_name = "Danny Washurn"
                if player2_name == "Danny Washburn":
                    player2_name = "Danny Washurn"
                
                player1 = find_player_by_name(players, player1_name)
                player2 = find_player_by_name(players, player2_name)
                
                if not player1:
                    print(f"    ❌ Player '{player1_name}' not found")
                    failed_imports += 1
                    continue
                    
                if not player2:
                    print(f"    ❌ Player '{player2_name}' not found")
                    failed_imports += 1
                    continue
                
                # Create the matchup
                result = create_matchup(week_obj['id'], player1['id'], player2['id'])
                
                if result:
                    successful_imports += 1
                else:
                    print(f"    ❌ Failed to create matchup: {player1_name} vs {player2_name}")
                    failed_imports += 1
    
    print(f"✅ Import completed: {successful_imports} successful, {failed_imports} failed")
    return failed_imports == 0

def main():
    """Main function to clear and import Session 3 matchups"""
    print("🏌️ Session 3 Matchup Import Tool")
    print("=" * 50)
    
    # Step 1: Clear existing matchups
    if not clear_session3_matchups():
        print("❌ Failed to clear existing matchups. Aborting.")
        return
    
    print()
    
    # Step 2: Import new matchups
    if not import_new_matchups():
        print("❌ Import completed with errors")
        return
    
    print()
    print("🎉 Session 3 matchup import completed successfully!")

if __name__ == "__main__":
    main()
