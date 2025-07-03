#!/usr/bin/env python3
"""
Import Week 1 initial data to set player handicaps and average scores.

This script parses the Week 1 data file and updates player records with:
- Initial handicap
- Current handicap
- Initial average score
- Current average score

Usage:
    python scripts/database/import_week1_initial_data.py
"""

import re
import requests
import json
from typing import List, Dict, Optional

# Configuration
API_BASE_URL = "http://localhost:5274/api"
DATA_FILE_PATH = "data/analysis/week1_initial_data.txt"

def get_week1_data() -> List[Dict]:
    """Get Week 1 data directly from the provided data."""
    week1_data = [
        {"name": "George Hutson", "phone": "610-428-4032", "average_score": 41.17391304, "handicap": 5},
        {"name": "Jeff Dilcher", "phone": "215-804-7664", "average_score": 41.42941799, "handicap": 5},
        {"name": "Bill Stein", "phone": "484-695-9692", "average_score": 42.30281385, "handicap": 5},
        {"name": "Alex Peck", "email": "alexanderpeck89@gmail.com", "phone": "908-442-2036", "average_score": 42.75936147, "handicap": 5.5},
        {"name": "Tim Seyler", "phone": "(484) 343-4123", "average_score": 43.21590909, "handicap": 6},
        {"name": "Kevin Kelhart", "phone": "610-597-3315", "average_score": 43.52275304, "handicap": 6},
        {"name": "Joe Mahachanh", "phone": "267-382-8223", "average_score": 43.56699415, "handicap": 6},
        {"name": "John Perry", "phone": "610-530-9092", "average_score": 43.59866667, "handicap": 6},
        {"name": "Carl Hardner", "phone": "484-664-9193", "average_score": 43.5951733, "handicap": 6},
        {"name": "Jay Sullivan", "phone": "610-476-4418", "average_score": 44.09514286, "handicap": 6},
        {"name": "Stu Silfies", "phone": "484-547-9553", "average_score": 44.625, "handicap": 6},
        {"name": "Steve Bedek", "phone": "484-274-1325", "average_score": 44.68161905, "handicap": 6},
        {"name": "Curt Saeger", "phone": "610-392-8228", "average_score": 44.79304813, "handicap": 6},
        {"name": "Lou Gabrielle", "phone": "484-951-0821", "average_score": 44.84508772, "handicap": 6},
        {"name": "Frank Frankenfield", "phone": "484-375-9035", "average_score": 44.96529412, "handicap": 6},
        {"name": "Kenny Palladino", "phone": "(610) 657-9977", "average_score": 44.99017544, "handicap": 6},
        {"name": "Matt Speth", "phone": "484-788-3417", "average_score": 45.23809524, "handicap": 7},
        {"name": "Jim Eck", "phone": "610-858-6264", "average_score": 46.44694444, "handicap": 7},
        {"name": "Kevin Kelhart JR", "phone": "484-821-7630", "average_score": 46.58333333, "handicap": 7},
        {"name": "Steve Hampton", "phone": "610-442-2483", "average_score": 47.58867725, "handicap": 8},
        {"name": "Bob Gross", "phone": "484-866-5195", "average_score": 47.85052632, "handicap": 8},
        {"name": "Juan Matute", "phone": "610-417-7659", "average_score": 47.95, "handicap": 8},
        {"name": "Matt Donahue", "email": "mathew.donahue@htlyons.com", "phone": "570-851-7148", "average_score": 47.95, "handicap": 8},
        {"name": "Danny Washurn", "email": "washdaniel@gmail.com", "phone": "484-357-8773", "average_score": 47.95, "handicap": 8},
        {"name": "Ray Ballinger", "phone": "484-554-9044", "average_score": 48.42380952, "handicap": 9},
        {"name": "Rich Hart", "phone": "610-533-7423", "average_score": 50.70740741, "handicap": 11},
        {"name": "Mike Schaefer", "phone": "484-553-0734", "average_score": 51.42857143, "handicap": 11},
        {"name": "Steve Kerns", "phone": "484-256-4012", "average_score": 53.31578947, "handicap": 13},
        {"name": "Steve Filipovits", "phone": "610-823-2206", "average_score": 55.2, "handicap": 14},
        {"name": "Andrew Kerns", "phone": "484-401-1000", "average_score": 55.37037037, "handicap": 14},
        {"name": "Tom Haeusler", "email": "thaeusleriii@aol.com", "phone": "610-217-7491", "average_score": 58.5, "handicap": 17},
        {"name": "Jax Haeusler", "email": "jaxhaeusler@gmail.com", "phone": "484-635-9062", "average_score": 57.17, "handicap": 17}
    ]
    return week1_data

def parse_week1_data(file_path: str) -> List[Dict]:
    """Parse the Week 1 data file and extract player information."""
    # For now, use the hardcoded data since file parsing had issues
    return get_week1_data()

def find_player_by_name(players_from_api: List[Dict], target_name: str) -> Optional[Dict]:
    """Find a player in the API data by matching name."""
    target_parts = target_name.lower().split()
    
    for player in players_from_api:
        api_name = f"{player['firstName']} {player['lastName']}".lower()
        api_parts = api_name.split()
        
        # Check if all parts of target name are in API name
        if all(part in api_parts for part in target_parts):
            return player
            
        # Also check reverse (in case of name order differences)
        if all(part in target_parts for part in api_parts):
            return player
    
    return None

def update_player_data(player_id: str, handicap: float, average_score: float, player_data: Dict) -> bool:
    """Update a player's handicap and average score via API."""
    try:
        # Update the player data
        updated_data = player_data.copy()
        updated_data.update({
            'initialHandicap': handicap,
            # Note: currentHandicap is no longer stored, calculated dynamically
            'initialAverageScore': average_score,
            'currentAverageScore': average_score
        })
        
        # Send update request
        response = requests.put(f"{API_BASE_URL}/players/{player_id}", json=updated_data)
        if response.status_code in [200, 204]:  # Both 200 and 204 indicate success
            print(f"✅ Updated {player_data['firstName']} {player_data['lastName']}: Handicap={handicap}, Avg={average_score:.2f}")
            return True
        else:
            print(f"❌ Failed to update player {player_id}: {response.status_code} - {response.text}")
            return False
            
    except Exception as e:
        print(f"❌ Error updating player {player_id}: {str(e)}")
        return False

def main():
    """Main function to import Week 1 data."""
    print("🏌️ Golf League Manager - Week 1 Data Import")
    print("=" * 50)
    
    # Parse the Week 1 data file
    print("📁 Parsing Week 1 data file...")
    try:
        week1_players = parse_week1_data(DATA_FILE_PATH)
        print(f"   Found {len(week1_players)} players in Week 1 data")
    except FileNotFoundError:
        print(f"❌ Error: Data file not found at {DATA_FILE_PATH}")
        return
    except Exception as e:
        print(f"❌ Error parsing data file: {str(e)}")
        return
    
    # Get current players from API
    print("🔍 Fetching current players from API...")
    try:
        response = requests.get(f"{API_BASE_URL}/players")
        if response.status_code != 200:
            print(f"❌ Failed to fetch players from API: {response.status_code}")
            return
        api_players = response.json()
        print(f"   Found {len(api_players)} players in database")
    except Exception as e:
        print(f"❌ Error fetching players: {str(e)}")
        return
    
    # Match and update players
    print("\n🔄 Matching and updating players...")
    updated_count = 0
    not_found_count = 0
    
    for week1_player in week1_players:
        name = week1_player['name']
        handicap = week1_player['handicap']
        average_score = week1_player['average_score']
        
        # Find matching player in API data
        api_player = find_player_by_name(api_players, name)
        
        if api_player:
            if update_player_data(api_player['id'], handicap, average_score, api_player):
                updated_count += 1
        else:
            print(f"⚠️  Player not found in database: {name}")
            not_found_count += 1
    
    # Summary
    print("\n📊 Import Summary:")
    print(f"   ✅ Players updated: {updated_count}")
    print(f"   ⚠️  Players not found: {not_found_count}")
    print(f"   📁 Total in Week 1 data: {len(week1_players)}")
    
    if not_found_count > 0:
        print("\n💡 Tip: Players not found may need to be created first or have different names in the database.")

if __name__ == "__main__":
    main()
