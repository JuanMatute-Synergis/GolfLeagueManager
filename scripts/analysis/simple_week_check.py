#!/usr/bin/env python3
import sys
import json

print("Starting analysis...", flush=True)

try:
    with open('score_entries_data.json', 'r') as f:
        data = json.load(f)
    print(f"Loaded {len(data)} entries", flush=True)
    
    # Check week structure
    weeks = set()
    for entry in data:
        weeks.add(entry['weekId'])
    
    print(f"Unique weeks: {len(weeks)}", flush=True)
    print("Week IDs:", flush=True)
    for week in sorted(weeks):
        print(f"  {week}", flush=True)
        
except Exception as e:
    print(f"Error: {e}", flush=True)
    sys.exit(1)

print("Analysis complete", flush=True)
