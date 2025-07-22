#!/usr/bin/env python3
import requests
import json

# Check Jay Sullivan's current data for Week 11
api_url = "http://localhost:5274/api/standings/session?seasonId=a57df491-9860-4c01-a883-ab68e838adb7&weekId=bf21e110-094c-4ca8-95de-da5d21ae340f"

response = requests.get(api_url)
data = response.json()

# Find Jay Sullivan
jay_data = None
for flight in data['flights']:
    for player in flight['players']:
        if 'Jay' in player['name']:
            jay_data = player
            break
    if jay_data:
        break

print("=== JAY SULLIVAN DEBUG INFO ===")
print(f"Name: {jay_data['name']}")
print(f"Gross Score: {jay_data['grossScore']}")
print(f"This Week Points: {jay_data['thisWeekPoints']}")
print(f"Is Absent: {jay_data['isAbsent']}")
print(f"Average Score: {jay_data['averageScore']}")
print(f"Handicap: {jay_data['handicap']}")

# Expected for "absent with no notice": 
# grossScore: 0, thisWeekPoints: 0, isAbsent: true
# Actual: grossScore: 44, thisWeekPoints: 4, isAbsent: true

print("\n=== ANALYSIS ===")
if jay_data['isAbsent'] and jay_data['thisWeekPoints'] == 4:
    print("❌ BUG CONFIRMED: Jay is marked absent but has 4 points (should be 0 for no notice)")
    print("   This indicates 'AbsentWithNotice' is set to TRUE in database")
elif jay_data['isAbsent'] and jay_data['thisWeekPoints'] == 0:
    print("✅ CORRECT: Jay is absent with no notice (0 points)")
else:
    print("? UNCLEAR: Jay appears to not be absent")

if jay_data['grossScore'] > 0 and jay_data['isAbsent']:
    print("❌ INCONSISTENCY: Jay has a gross score but is marked absent")
