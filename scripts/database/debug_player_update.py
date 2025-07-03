#!/usr/bin/env python3
"""
Debug script to investigate why initialAverageScore is not being set correctly.
"""

import requests
import json

# Configuration
API_BASE_URL = "http://localhost:5274/api"

def test_single_player_update():
    """Test updating a single player to debug the issue."""
    
    # Get Juan Matute's current data
    print("🔍 Fetching all players...")
    response = requests.get(f"{API_BASE_URL}/players")
    players = response.json()
    
    juan = None
    for player in players:
        if player['firstName'] == 'Juan' and player['lastName'] == 'Matute':
            juan = player
            break
    
    if not juan:
        print("❌ Juan Matute not found")
        return
    
    print(f"\n📊 Juan's CURRENT data:")
    print(f"   Initial Handicap: {juan.get('initialHandicap')}")
    print(f"   Note: Current handicap is now calculated dynamically from scores")
    print(f"   Initial Average Score: {juan.get('initialAverageScore')}")
    print(f"   Current Average Score: {juan.get('currentAverageScore')}")
    
    # Prepare the update with our Week 1 values
    week1_handicap = 8.0
    week1_average = 47.95
    
    updated_data = juan.copy()
    updated_data.update({
        'initialHandicap': week1_handicap,
        # Note: currentHandicap is no longer stored, calculated dynamically
        'initialAverageScore': week1_average,
        'currentAverageScore': week1_average
    })
    
    print(f"\n📤 Sending UPDATE with:")
    print(f"   Initial Handicap: {week1_handicap}")
    print(f"   Note: Current handicap will be calculated dynamically")
    print(f"   Initial Average Score: {week1_average}")
    print(f"   Current Average Score: {week1_average}")
    
    # Show the exact JSON being sent
    print(f"\n📋 Full JSON payload:")
    print(json.dumps(updated_data, indent=2))
    
    # Send the update
    response = requests.put(f"{API_BASE_URL}/players/{juan['id']}", json=updated_data)
    print(f"\n📨 API Response: {response.status_code}")
    if response.text:
        print(f"   Response text: {response.text}")
    
    # Check what we get back
    print("\n🔍 Fetching Juan's data AFTER update...")
    response = requests.get(f"{API_BASE_URL}/players")
    players = response.json()
    
    juan_after = None
    for player in players:
        if player['firstName'] == 'Juan' and player['lastName'] == 'Matute':
            juan_after = player
            break
    
    if juan_after:
        print(f"\n📊 Juan's AFTER UPDATE data:")
        print(f"   Initial Handicap: {juan_after.get('initialHandicap')} {'✅' if juan_after.get('initialHandicap') == week1_handicap else '❌'}")
        print(f"   Note: Current handicap is now calculated dynamically from scores")
        print(f"   Initial Average Score: {juan_after.get('initialAverageScore')} {'✅' if juan_after.get('initialAverageScore') == week1_average else '❌'}")
        print(f"   Current Average Score: {juan_after.get('currentAverageScore')} {'✅' if juan_after.get('currentAverageScore') == week1_average else '❌'}")
        
        print(f"\n🔍 Comparison:")
        print(f"   Expected Initial Avg: {week1_average}")
        print(f"   Actual Initial Avg: {juan_after.get('initialAverageScore')}")
        print(f"   Difference: {abs(week1_average - juan_after.get('initialAverageScore', 0))}")

if __name__ == "__main__":
    test_single_player_update()
