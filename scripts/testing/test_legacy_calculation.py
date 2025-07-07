#!/usr/bin/env python3
"""
Test script to debug the legacy average calculation issue.
"""

import requests
import json

BASE_URL = "http://localhost:5274"
KEVIN_ID = "1cbaa69a-f72e-4ccd-9e6b-2ea1a501f5ee"
SEASON_ID = "ad3e9a12-7b6c-4d84-9f7a-dac8fc9e1b36"

def test_kevin_stats():
    """Test Kevin Kelhart's stats"""
    url = f"{BASE_URL}/api/AverageScore/player/{KEVIN_ID}/season/{SEASON_ID}/stats"
    response = requests.get(url)
    
    if response.status_code == 200:
        data = response.json()
        print("Kevin Kelhart Stats:")
        print(f"  Initial Average: {data['initialAverageScore']}")
        print(f"  Current Average: {data['currentAverageScore']}")
        print(f"  Rounds Played: {data['roundsPlayed']}")
        print(f"  Calculated Average: {data['averageScoreCalculated']}")
        return data
    else:
        print(f"Error getting stats: {response.status_code} - {response.text}")
        return None

def test_update_average(week_number=1):
    """Test updating Kevin's average up to a specific week"""
    url = f"{BASE_URL}/api/AverageScore/player/{KEVIN_ID}/season/{SEASON_ID}/update-to-week/{week_number}"
    response = requests.post(url)
    
    if response.status_code == 200:
        average = response.json()
        print(f"Kevin's average up to week {week_number}: {average}")
        return average
    else:
        print(f"Error updating average: {response.status_code} - {response.text}")
        return None

def get_kevin_scores():
    """Get Kevin's actual scores from matchups"""
    url = f"{BASE_URL}/api/matchups"
    response = requests.get(url)
    
    if response.status_code == 200:
        matchups = response.json()
        kevin_scores = []
        
        for matchup in matchups:
            if matchup.get('playerAId') == KEVIN_ID and matchup.get('playerAScore'):
                kevin_scores.append({
                    'week': matchup.get('weekNumber', 'unknown'),
                    'score': matchup.get('playerAScore'),
                    'weekId': matchup.get('weekId')
                })
            elif matchup.get('playerBId') == KEVIN_ID and matchup.get('playerBScore'):
                kevin_scores.append({
                    'week': matchup.get('weekNumber', 'unknown'),
                    'score': matchup.get('playerBScore'),
                    'weekId': matchup.get('weekId')
                })
        
        print("Kevin's actual scores:")
        for score in sorted(kevin_scores, key=lambda x: x['week']):
            print(f"  Week {score['week']}: {score['score']}")
        
        return kevin_scores
    else:
        print(f"Error getting matchups: {response.status_code}")
        return []

def main():
    print("Testing Kevin Kelhart's legacy average calculation...")
    print("=" * 60)
    
    # Get basic stats
    stats = test_kevin_stats()
    print()
    
    # Try to get actual scores
    scores = get_kevin_scores()
    print()
    
    # Test updating averages for different weeks
    for week in [1, 2, 3]:
        avg = test_update_average(week)
        if avg is not None:
            print()

if __name__ == "__main__":
    main()
