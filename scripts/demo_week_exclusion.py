#!/usr/bin/env python3
"""
Demonstration script showing how to exclude weeks from handicap and average score calculations
"""

import requests
import json

BASE_URL = "http://localhost:5274/api"

def update_week_calculation_settings(week_id, counts_for_scoring=True, counts_for_handicap=True):
    """Update a week's calculation settings"""
    # First get the current week data
    response = requests.get(f"{BASE_URL}/weeks/{week_id}")
    if response.status_code != 200:
        print(f"❌ Failed to get week {week_id}")
        return False
    
    week_data = response.json()
    
    # Update the calculation settings
    week_data['countsForScoring'] = counts_for_scoring
    week_data['countsForHandicap'] = counts_for_handicap
    
    # Send the update
    response = requests.put(f"{BASE_URL}/weeks/{week_id}", json=week_data)
    if response.status_code == 200:
        print(f"✅ Week {week_data['weekNumber']} updated:")
        print(f"   - Counts for Scoring: {counts_for_scoring}")
        print(f"   - Counts for Handicap: {counts_for_handicap}")
        return True
    else:
        print(f"❌ Failed to update week {week_id}: {response.status_code}")
        return False

def test_calculation_exclusion():
    """Test excluding weeks from calculations"""
    print("=== WEEK CALCULATION EXCLUSION DEMO ===\n")
    
    # Get seasons and weeks
    seasons_response = requests.get(f"{BASE_URL}/seasons")
    if seasons_response.status_code != 200:
        print("❌ Failed to get seasons")
        return
    
    season_id = seasons_response.json()[0]["id"]
    print(f"Using season: {season_id}")
    
    weeks_response = requests.get(f"{BASE_URL}/weeks", params={"seasonId": season_id})
    if weeks_response.status_code != 200:
        print("❌ Failed to get weeks")
        return
    
    weeks = weeks_response.json()
    print(f"Found {len(weeks)} weeks\n")
    
    # Show current settings
    print("📊 Current week calculation settings:")
    for week in weeks[:5]:  # Show first 5 weeks
        print(f"   Week {week['weekNumber']}: Scoring={week['countsForScoring']}, Handicap={week['countsForHandicap']}")
    
    print("\n🔧 Example: Excluding Week 3 from scoring calculations (holiday week)...")
    
    # Find week 3
    week_3 = next((w for w in weeks if w['weekNumber'] == 3), None)
    if week_3:
        # Exclude week 3 from scoring but keep for handicap
        update_week_calculation_settings(
            week_3['id'], 
            counts_for_scoring=False,  # Exclude from scoring
            counts_for_handicap=True   # Keep for handicap
        )
    
    print("\n🔧 Example: Excluding Week 5 from both calculations (tournament week)...")
    
    # Find week 5
    week_5 = next((w for w in weeks if w['weekNumber'] == 5), None)
    if week_5:
        # Exclude week 5 from both calculations
        update_week_calculation_settings(
            week_5['id'], 
            counts_for_scoring=False,  # Exclude from scoring
            counts_for_handicap=False  # Exclude from handicap
        )
    
    print("\n📊 Updated week calculation settings:")
    # Refresh weeks data
    weeks_response = requests.get(f"{BASE_URL}/weeks", params={"seasonId": season_id})
    weeks = weeks_response.json()
    
    for week in weeks[:6]:  # Show first 6 weeks
        scoring_status = "✅" if week['countsForScoring'] else "❌"
        handicap_status = "✅" if week['countsForHandicap'] else "❌"
        print(f"   Week {week['weekNumber']}: Scoring {scoring_status}, Handicap {handicap_status}")
    
    print("\n💡 Impact on calculations:")
    print("   - Week 3: Scores will be ignored in standings/points, but used for handicap")
    print("   - Week 5: Scores will be ignored in both standings and handicap calculations")
    print("   - Other weeks: Normal calculation behavior")
    
    print("\n🔄 Restoring original settings...")
    
    # Restore original settings
    if week_3:
        update_week_calculation_settings(week_3['id'], True, True)
    if week_5:
        update_week_calculation_settings(week_5['id'], True, True)
    
    print("\n✅ Demo complete! Use the week management UI to configure these settings.")

def show_usage_examples():
    """Show common usage examples"""
    print("\n=== COMMON USAGE EXAMPLES ===")
    print()
    print("🎄 Holiday Week (Christmas, New Year):")
    print("   - Counts for Scoring: ❌ (no points awarded)")
    print("   - Counts for Handicap: ✅ (scores still matter for handicap)")
    print()
    print("🏆 Tournament Week:")
    print("   - Counts for Scoring: ❌ (different scoring system)")
    print("   - Counts for Handicap: ❌ (tournament conditions too different)")
    print()
    print("🌧️ Weather Makeup Week:")
    print("   - Counts for Scoring: ✅ (normal league play)")
    print("   - Counts for Handicap: ✅ (normal conditions)")
    print()
    print("🎯 Skills Challenge Week:")
    print("   - Counts for Scoring: ❌ (not regular golf)")
    print("   - Counts for Handicap: ❌ (not representative of ability)")

if __name__ == "__main__":
    test_calculation_exclusion()
    show_usage_examples()
