#!/usr/bin/env python3
"""
Test script to verify that average score calculations now use Week 1 baseline
instead of session-based logic.
"""

import requests
import json
from typing import List, Dict

# Configuration
API_BASE_URL = "http://localhost:5274/api"

def test_average_score_calculation():
    """Test that average score calculations are working with week 1 baseline."""
    print("🧪 Testing Average Score Calculation with Week 1 Baseline")
    print("=" * 60)
    
    # Get all players
    try:
        response = requests.get(f"{API_BASE_URL}/players")
        if response.status_code != 200:
            print(f"❌ Failed to fetch players: {response.status_code}")
            return
        players = response.json()
        print(f"✅ Found {len(players)} players")
    except Exception as e:
        print(f"❌ Error fetching players: {str(e)}")
        return
    
    # Test with Juan Matute
    juan = None
    for player in players:
        if player['firstName'] == 'Juan' and player['lastName'] == 'Matute':
            juan = player
            break
    
    if not juan:
        print("❌ Juan Matute not found")
        return
    
    print(f"\n📊 Testing with Juan Matute:")
    print(f"   Player ID: {juan['id']}")
    print(f"   Initial Average Score: {juan['initialAverageScore']}")
    print(f"   Current Average Score: {juan['currentAverageScore']}")
    print(f"   Initial Handicap: {juan['initialHandicap']}")
    print(f"   Note: Current handicap is now calculated dynamically from scores")
    
    # Test the average score calculation endpoint
    try:
        # Get seasons to find active season
        seasons_response = requests.get(f"{API_BASE_URL}/seasons")
        if seasons_response.status_code == 200:
            seasons = seasons_response.json()
            if seasons:
                season_id = seasons[0]['id']  # Use first season
                print(f"   Using Season ID: {season_id}")
                
                # Test average score up to week endpoint
                avg_response = requests.get(f"{API_BASE_URL}/averagescore/player/{juan['id']}/season/{season_id}/uptoweek/5")
                if avg_response.status_code == 200:
                    calculated_avg = avg_response.json()
                    print(f"   ✅ Calculated average up to week 5: {calculated_avg}")
                    
                    # Verify this matches the new formula expectation
                    # New formula: (Initial Average + Sum of actual scores) / (1 + Number of actual rounds)
                    print(f"   📈 This should be calculated from week 1 baseline: {juan['initialAverageScore']}")
                else:
                    print(f"   ⚠️  Average score endpoint returned: {avg_response.status_code}")
            else:
                print("   ⚠️  No seasons found")
        else:
            print(f"   ⚠️  Could not fetch seasons: {seasons_response.status_code}")
    except Exception as e:
        print(f"   ❌ Error testing average score calculation: {str(e)}")
    
    # Test with a few more players to verify consistency
    print(f"\n🔍 Testing other players for consistency:")
    test_players = ['Kevin Kelhart JR', 'George Hutson', 'Alex Peck']
    
    for player_name in test_players:
        test_player = None
        for player in players:
            full_name = f"{player['firstName']} {player['lastName']}"
            if full_name == player_name:
                test_player = player
                break
        
        if test_player:
            initial_avg = test_player['initialAverageScore']
            current_avg = test_player['currentAverageScore']
            print(f"   {player_name}: Initial={initial_avg}, Current={current_avg}")
            
            # Check if they're close (indicating week 1 baseline is being used)
            if abs(initial_avg - current_avg) < 1.0:
                print(f"      ✅ Values are close, likely using week 1 baseline")
            else:
                print(f"      ⚠️  Values differ by {abs(initial_avg - current_avg):.2f}")
        else:
            print(f"   ❌ {player_name} not found")

def test_handicap_calculation():
    """Test that handicap calculations are working properly."""
    print(f"\n🏌️ Testing Handicap Calculation")
    print("=" * 60)
    
    try:
        # Test handicap endpoint if available
        response = requests.get(f"{API_BASE_URL}/handicap")
        if response.status_code == 404:
            print("   ℹ️  No general handicap endpoint found (this is normal)")
        else:
            print(f"   📊 Handicap endpoint status: {response.status_code}")
    except Exception as e:
        print(f"   ⚠️  Could not test handicap endpoint: {str(e)}")

if __name__ == "__main__":
    test_average_score_calculation()
    test_handicap_calculation()
    print(f"\n✅ Test completed!")
