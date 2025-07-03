#!/usr/bin/env python3

import json
import pandas as pd

def main():
    print("Starting week analysis...")
    
    try:
        # Load score entries
        with open('score_entries_data.json', 'r') as f:
            score_entries = json.load(f)
        print(f"Loaded {len(score_entries)} score entries")
        
        # Load comparison data
        comparison_df = pd.read_csv('handicap_comparison.csv')
        print(f"Loaded comparison data for {len(comparison_df)} players")
        
        # Analyze weeks
        weeks = set()
        for entry in score_entries:
            weeks.add(entry['weekId'])
        
        print(f"Found weeks: {sorted(weeks)}")
        
        # Check a specific player's progression
        target_player = "Kevin Kelhart JR"
        player_scores = []
        
        for entry in score_entries:
            if entry['player']['name'] == target_player and entry['score'] > 0:
                player_scores.append({
                    'week': entry['weekId'],
                    'score': entry['score'],
                    'points': entry['pointsEarned']
                })
        
        player_scores.sort(key=lambda x: x['week'])
        
        print(f"\n{target_player} scores by week:")
        for score_data in player_scores:
            print(f"Week {score_data['week']}: Score {score_data['score']}, Points {score_data['points']}")
        
        # Get this player's handicap info
        player_row = comparison_df[comparison_df['Player Name'] == target_player]
        if not player_row.empty:
            print(f"\nSheet Handicap: {player_row.iloc[0]['Sheet Handicap']}")
            print(f"System Handicap: {player_row.iloc[0]['System Current Handicap']}")
            print(f"Difference: {player_row.iloc[0]['Handicap Difference']}")
        
    except Exception as e:
        print(f"Error: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    main()
