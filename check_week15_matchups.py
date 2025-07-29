#!/usr/bin/env python3

import requests
import json

# Configuration
API_BASE_URL = "http://localhost:5274/api"
SEASON_ID = "a57df491-9860-4c01-a883-ab68e838adb7"

def get_players():
    """Get all players for the season"""
    try:
        response = requests.get(f"{API_BASE_URL}/players/season/{SEASON_ID}")
        response.raise_for_status()
        return response.json()
    except requests.RequestException as e:
        print(f"Error fetching players: {e}")
        return []

def get_flight_assignments():
    """Get flight assignments for Session 3 (Week 15)"""
    try:
        # Use the standings API to get current session assignments
        response = requests.get(f"{API_BASE_URL}/standings/session?seasonId={SEASON_ID}&sessionStartWeek=15")
        response.raise_for_status()
        standings = response.json()
        
        # Extract flight assignments from standings
        assignments = {}
        for standing in standings:
            player_id = standing['playerId']
            flight_number = standing['flightNumber']
            assignments[player_id] = {
                'playerName': standing['playerName'],
                'flight': flight_number
            }
        
        return assignments
        
    except requests.RequestException as e:
        print(f"Error fetching flight assignments: {e}")
        return {}

def check_week15_matchups():
    print("=== Week 15 Matchup Analysis ===\n")
    
    # Get Week 15 data
    try:
        response = requests.get(f"{API_BASE_URL}/weeks/season/{SEASON_ID}")
        response.raise_for_status()
        weeks = response.json()
        
        week15 = None
        for week in weeks:
            if week['weekNumber'] == 15:
                week15 = week
                break
                
        if not week15:
            print("Week 15 not found!")
            return
            
        matchups = week15['matchups']
        
    except requests.RequestException as e:
        print(f"Error fetching weeks: {e}")
        return
    
    # Get players and flight assignments
    players = get_players()
    flight_assignments = get_flight_assignments()
    
    # Create player lookup
    player_lookup = {}
    for player in players:
        player_lookup[player['id']] = player
    
    print(f"Found {len(matchups)} matchups for Week 15")
    print(f"Found {len(flight_assignments)} flight assignments for Session 3\n")
    
    # Group assignments by flight
    flights = {}
    for player_id, assignment in flight_assignments.items():
        flight = assignment['flight']
        if flight not in flights:
            flights[flight] = []
        flights[flight].append(assignment['playerName'])
    
    print("Session 3 (Week 15) Flight Assignments:")
    for flight_num in sorted(flights.keys()):
        print(f"Flight {flight_num}: {len(flights[flight_num])} players")
        for player in sorted(flights[flight_num]):
            print(f"  - {player}")
        print()
    
    # Check each matchup
    print("=== Matchup Analysis ===\n")
    problematic_matchups = []
    
    for matchup in matchups:
        player_a_id = matchup['playerAId']
        player_b_id = matchup['playerBId']
        
        player_a = player_lookup.get(player_a_id, {})
        player_b = player_lookup.get(player_b_id, {})
        
        player_a_name = f"{player_a.get('firstName', 'Unknown')} {player_a.get('lastName', 'Player')}"
        player_b_name = f"{player_b.get('firstName', 'Unknown')} {player_b.get('lastName', 'Player')}"
        
        player_a_assignment = flight_assignments.get(player_a_id)
        player_b_assignment = flight_assignments.get(player_b_id)
        
        status = "✓"
        issues = []
        
        if not player_a_assignment:
            issues.append(f"{player_a_name} not assigned to any flight")
            status = "✗"
        
        if not player_b_assignment:
            issues.append(f"{player_b_name} not assigned to any flight")
            status = "✗"
            
        if player_a_assignment and player_b_assignment:
            if player_a_assignment['flight'] != player_b_assignment['flight']:
                issues.append(f"Different flights: {player_a_name} (Flight {player_a_assignment['flight']}) vs {player_b_name} (Flight {player_b_assignment['flight']})")
                status = "✗"
        
        if issues:
            problematic_matchups.append({
                'matchup': f"{player_a_name} vs {player_b_name}",
                'issues': issues
            })
            
        print(f"{status} {player_a_name} vs {player_b_name}")
        if issues:
            for issue in issues:
                print(f"    - {issue}")
        else:
            flight_num = player_a_assignment['flight'] if player_a_assignment else "Unknown"
            print(f"    - Both in Flight {flight_num}")
        print()
    
    print("=== Summary ===")
    print(f"Total matchups: {len(matchups)}")
    print(f"Problematic matchups: {len(problematic_matchups)}")
    
    if problematic_matchups:
        print("\nProblematic matchups:")
        for issue in problematic_matchups:
            print(f"- {issue['matchup']}")
            for detail in issue['issues']:
                print(f"  • {detail}")
    else:
        print("All matchups appear to be valid!")
    
    # Look specifically for Matt players
    print("\n=== Looking for Matt players ===")
    matt_players = [p for p in players if 'matt' in p.get('firstName', '').lower()]
    
    print(f"Found {len(matt_players)} Matt players:")
    for player in matt_players:
        player_id = player['id']
        full_name = f"{player.get('firstName', '')} {player.get('lastName', '')}"
        assignment = flight_assignments.get(player_id)
        if assignment:
            print(f"- {full_name} (ID: {player_id[:8]}...) - Flight {assignment['flight']}")
        else:
            print(f"- {full_name} (ID: {player_id[:8]}...) - NO FLIGHT ASSIGNMENT")

if __name__ == "__main__":
    check_week15_matchups()
