#!/usr/bin/env python3

import json
import pandas as pd
from collections import defaultdict
import statistics

def load_data():
    # Load score entries
    with open('score_entries_data.json', 'r') as f:
        score_entries = json.load(f)
    
    # Load comparison data
    comparison_df = pd.read_csv('handicap_comparison.csv')
    
    return score_entries, comparison_df

def analyze_week_progression():
    score_entries, comparison_df = load_data()
    
    print("=== WEEK ANALYSIS ===")
    
    # Group by week ID to understand the week structure
    week_counts = defaultdict(int)
    week_scores = defaultdict(list)
    
    for entry in score_entries:
        week_id = entry['weekId']
        score = entry['score']
        week_counts[week_id] += 1
        if score > 0:  # Only count actual scores, not missed weeks
            week_scores[week_id].append(score)
    
    print(f"Total weeks found: {len(week_counts)}")
    print("Week ID -> Player count (actual scores):")
    
    # Sort by week ID to see progression
    sorted_weeks = sorted(week_counts.keys())
    for week_id in sorted_weeks:
        actual_scores = len(week_scores[week_id])
        total_entries = week_counts[week_id]
        print(f"Week {week_id}: {actual_scores} scores out of {total_entries} entries")
    
    return sorted_weeks, week_scores

def analyze_player_progression(player_name):
    score_entries, comparison_df = load_data()
    
    # Find the player in comparison data
    player_row = comparison_df[comparison_df['Player Name'] == player_name]
    if player_row.empty:
        print(f"Player {player_name} not found in comparison data")
        return
    
    # Get player's scores by week
    player_scores = []
    for entry in score_entries:
        if entry['player']['name'] == player_name and entry['score'] > 0:
            player_scores.append({
                'week_id': entry['weekId'],
                'score': entry['score'],
                'points': entry['pointsEarned']
            })
    
    # Sort by week
    player_scores.sort(key=lambda x: x['week_id'])
    
    print(f"\n=== {player_name} PROGRESSION ===")
    print(f"Sheet Handicap: {player_row.iloc[0]['Sheet Handicap']}")
    print(f"System Handicap: {player_row.iloc[0]['System Current Handicap']}")
    print(f"Difference: {player_row.iloc[0]['Handicap Difference']}")
    
    print("\nWeek-by-week scores:")
    running_scores = []
    for score_data in player_scores:
        running_scores.append(score_data['score'])
        running_avg = sum(running_scores) / len(running_scores)
        print(f"Week {score_data['week_id']}: Score {score_data['score']}, Points {score_data['points']}, Running Avg: {running_avg:.1f}")
    
    return player_scores

def calculate_handicap_methodologies():
    score_entries, comparison_df = load_data()
    
    print("\n=== DETAILED METHODOLOGY ANALYSIS ===")
    
    # Group scores by player
    player_scores = defaultdict(list)
    for entry in score_entries:
        if entry['score'] > 0:  # Only actual scores
            player_scores[entry['player']['name']].append({
                'week_id': entry['weekId'],
                'score': entry['score'],
                'handicap': entry['player']['handicap']
            })
    
    # Sort each player's scores by week
    for player in player_scores:
        player_scores[player].sort(key=lambda x: x['week_id'])
    
    methodology_errors = {
        'overall_avg': [],
        'recent_5': [],
        'recent_3': [],
        'best_3': [],
        'best_5': [],
        'best_8_of_20': [],  # Traditional USGA method
        'weighted_recent': []  # More weight to recent rounds
    }
    
    for _, row in comparison_df.iterrows():
        player_name = row['Player Name']
        sheet_handicap = row['Sheet Handicap']
        
        if player_name not in player_scores:
            continue
            
        scores = [s['score'] for s in player_scores[player_name]]
        if len(scores) < 3:
            continue
            
        # Calculate different methodologies
        methods = {}
        
        # Overall average
        methods['overall_avg'] = sum(scores) / len(scores)
        
        # Recent rounds
        if len(scores) >= 5:
            methods['recent_5'] = sum(scores[-5:]) / 5
        if len(scores) >= 3:
            methods['recent_3'] = sum(scores[-3:]) / 3
            
        # Best rounds
        sorted_scores = sorted(scores)
        if len(scores) >= 3:
            methods['best_3'] = sum(sorted_scores[:3]) / 3
        if len(scores) >= 5:
            methods['best_5'] = sum(sorted_scores[:5]) / 5
        if len(scores) >= 8:
            methods['best_8_of_20'] = sum(sorted_scores[:8]) / 8
            
        # Weighted recent (70% recent 3, 30% overall)
        if len(scores) >= 3:
            recent_3_avg = sum(scores[-3:]) / 3
            overall_avg = sum(scores) / len(scores)
            methods['weighted_recent'] = (recent_3_avg * 0.7) + (overall_avg * 0.3)
        
        # Calculate sheet average based on sheet handicap
        # Assuming par 36 course, sheet_avg = par + handicap
        sheet_avg = 36 + sheet_handicap
        
        # Calculate errors for each method
        for method_name, calculated_avg in methods.items():
            error = abs(calculated_avg - sheet_avg)
            methodology_errors[method_name].append(error)
    
    # Print results
    print("Average error for each methodology:")
    for method, errors in methodology_errors.items():
        if errors:
            avg_error = sum(errors) / len(errors)
            print(f"{method}: {avg_error:.3f} strokes")
    
    return methodology_errors

def main():
    print("Starting detailed handicap analysis...")
    
    # Analyze week structure
    sorted_weeks, week_scores = analyze_week_progression()
    
    # Analyze specific players with large discrepancies
    problem_players = ['Kevin Kelhart JR', 'Ray Ballinger', 'Tom Haeusler', 'Jax Haeusler']
    
    for player in problem_players:
        analyze_player_progression(player)
    
    # Calculate methodology errors
    methodology_errors = calculate_handicap_methodologies()
    
    print("\n=== SUMMARY ===")
    print("The analysis suggests the previous system used overall average scoring")
    print("rather than the World Handicap System's more complex differential calculation.")
    print("This would explain why some players have discrepancies, especially those")
    print("with improving or declining trends in their recent play.")

if __name__ == "__main__":
    main()
