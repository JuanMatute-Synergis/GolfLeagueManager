#!/usr/bin/env python3
"""
Bulk assign players to flights for Session 3 using the matchup data
This script demonstrates how to assign all players from the week15_matchups.py file
"""

import requests
import json

# Flight assignments from week15_matchups.py
flight_assignments = {
    "1": [
        "George Hutson", "Rich Baker", "Jason Fink", "Bill Stein",
        "John Perry", "Alex Peck", "Jeff Dilcher", "Joe Mahachanh"
    ],
    "2": [
        "Tim Seyler", "Kevin Kelhart", "Curt Saeger", "Carl Hardner",
        "Kenny Palladino", "Frank Frankenfield", "Stu Silfies", "Ray Ballinger"
    ],
    "3": [
        "Steve Bedek", "Lou Gabrielle", "Jim Eck", "Danny Washurn",
        "Bob Gross", "Juan Matute", "Steve Hampton", "Kevin Kelhart JR"
    ],
    "4": [
        "Matt Donahue", "Rich Hart", "Andrew Kerns", "Steve Kerns",
        "Ben Mahachanh", "Jax Haeusler", "Steve Filipovits", "Tom Haeusler"
    ]
}

# Configuration
API_BASE_URL = "http://localhost:5274/api"
SEASON_ID = "a57df491-9860-4c01-a883-ab68e838adb7"  # 2025 season ID
SESSION_START_WEEK = 15  # Session 3 starts at week 15

def get_all_players():
    """Get all players from the API"""
    try:
        response = requests.get(f"{API_BASE_URL}/players")
        response.raise_for_status()
        return response.json()
    except requests.RequestException as e:
        print(f"Error fetching players: {e}")
        return []

def get_flights_by_season(season_id):
    """Get all flights for a season"""
    try:
        response = requests.get(f"{API_BASE_URL}/flights/season/{season_id}")
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
    """Find a flight by name"""
    for flight in flights:
        if flight['name'].lower() == flight_name.lower():
            return flight
    return None

def assign_player_to_flight(player_id, flight_id, season_id, session_start_week, is_flight_leader=False, handicap=0):
    """Assign a player to a flight"""
    assignment_data = {
        "playerId": player_id,
        "flightId": flight_id,
        "seasonId": season_id,
        "sessionStartWeekNumber": session_start_week,
        "isFlightLeader": is_flight_leader,
        "handicapAtAssignment": handicap
    }
    
    try:
        response = requests.post(f"{API_BASE_URL}/player-flight-assignments", json=assignment_data)
        response.raise_for_status()
        return response.json()
    except requests.RequestException as e:
        print(f"Error assigning player: {e}")
        return None

def bulk_assign_players():
    """Bulk assign all players to their flights"""
    print("Starting bulk player assignment...")
    
    # Get data from API
    print("Fetching players...")
    players = get_all_players()
    print(f"Found {len(players)} players")
    
    print("Fetching flights...")
    flights = get_flights_by_season(SEASON_ID)
    print(f"Found {len(flights)} flights")
    
    if not players or not flights:
        print("Failed to fetch required data. Exiting.")
        return
    
    # Process each flight
    successful_assignments = 0
    failed_assignments = 0
    
    for flight_name, player_names in flight_assignments.items():
        print(f"\nProcessing {flight_name}...")
        
        # Find the flight
        flight = find_flight_by_name(flights, flight_name)
        if not flight:
            print(f"  ERROR: Flight '{flight_name}' not found")
            continue
        
        flight_id = flight['id']
        print(f"  Found flight ID: {flight_id}")
        
        # Assign each player
        for i, player_name in enumerate(player_names):
            player = find_player_by_name(players, player_name)
            if not player:
                print(f"    ERROR: Player '{player_name}' not found")
                failed_assignments += 1
                continue
            
            # First player in each flight is the flight leader
            is_leader = (i == 0)
            
            # Assign the player
            result = assign_player_to_flight(
                player['id'], 
                flight_id, 
                SEASON_ID, 
                SESSION_START_WEEK,
                is_leader
            )
            
            if result:
                leader_text = " (Flight Leader)" if is_leader else ""
                print(f"    ✓ Assigned {player_name}{leader_text}")
                successful_assignments += 1
            else:
                print(f"    ✗ Failed to assign {player_name}")
                failed_assignments += 1
    
    print(f"\n{'='*50}")
    print(f"Bulk assignment completed!")
    print(f"Successful assignments: {successful_assignments}")
    print(f"Failed assignments: {failed_assignments}")
    print(f"{'='*50}")

if __name__ == "__main__":
    # Validate configuration
    if SEASON_ID == "YOUR_SEASON_ID_HERE":
        print("ERROR: Please set the SEASON_ID in the script before running")
        print("You can find the season ID in your Golf League Manager system")
        exit(1)
    
    bulk_assign_players()
