#!/usr/bin/env python3
import json

print("Starting analysis...")

try:
    with open('score_entries_data.json', 'r') as f:
        data = json.load(f)
    print(f"Loaded {len(data)} score entries")
    
    # Show sample data
    print("Sample entry:")
    print(json.dumps(data[0], indent=2))
    
    # Group by player
    players = {}
    for entry in data:
        player_id = entry['playerId']
        if player_id not in players:
            players[player_id] = []
        players[player_id].append(entry)
    
    print(f"Found {len(players)} unique players")
    
    # Show player with most entries
    max_entries = 0
    max_player = None
    for player_id, entries in players.items():
        if len(entries) > max_entries:
            max_entries = len(entries)
            max_player = player_id
    
    print(f"Player with most entries: {max_player} with {max_entries} entries")
    
except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()

print("Done!")
