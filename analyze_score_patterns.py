#!/usr/bin/env python3
"""
Analyze score entry patterns to understand handicap calculation methodology
of the previous golf league system.
"""

import json
import pandas as pd
from collections import defaultdict
import numpy as np

def load_score_data():
    """Load the score entry data retrieved from the API"""
    # This would be the data we retrieved earlier
    # For now, we'll read it from the saved file if available
    try:
        with open('score_entries_data.json', 'r') as f:
            return json.load(f)
    except FileNotFoundError:
        print("Score entries data not found. Need to fetch from API first.")
        return None

def analyze_week_progression(score_data):
    """Analyze score patterns by week for each player"""
    if not score_data:
        return None
    
    # Group by player and week
    player_weeks = defaultdict(lambda: defaultdict(list))
    
    for entry in score_data:
        player_id = entry['playerId']
        week_id = entry['weekId']
        score = entry.get('score', 0)
        points = entry.get('pointsEarned', 0)
        
        # Skip entries with no score (likely absent weeks)
        if score > 0:
            player_weeks[player_id][week_id].append({
                'score': score,
                'points': points,
                'player_name': entry.get('player', {}).get('firstName', '') + ' ' + 
                              entry.get('player', {}).get('lastName', ''),
                'current_handicap': entry.get('player', {}).get('currentHandicap', 0),
                'initial_handicap': entry.get('player', {}).get('initialHandicap', 0),
                'current_avg': entry.get('player', {}).get('currentAverageScore', 0),
                'initial_avg': entry.get('player', {}).get('initialAverageScore', 0)
            })
    
    return player_weeks

def calculate_progression_stats(player_weeks):
    """Calculate progression statistics for each player"""
    progression_stats = {}
    
    for player_id, weeks in player_weeks.items():
        if not weeks:
            continue
            
        # Get all scores for this player in chronological order
        all_scores = []
        player_name = ""
        handicap_info = {}
        
        for week_id, scores in weeks.items():
            for score_entry in scores:
                all_scores.append(score_entry['score'])
                if not player_name:
                    player_name = score_entry['player_name']
                    handicap_info = {
                        'current_handicap': score_entry['current_handicap'],
                        'initial_handicap': score_entry['initial_handicap'],
                        'current_avg': score_entry['current_avg'],
                        'initial_avg': score_entry['initial_avg']
                    }
        
        if len(all_scores) < 2:
            continue
            
        # Calculate various averages
        overall_avg = np.mean(all_scores)
        recent_5_avg = np.mean(all_scores[-5:]) if len(all_scores) >= 5 else np.mean(all_scores)
        recent_3_avg = np.mean(all_scores[-3:]) if len(all_scores) >= 3 else np.mean(all_scores)
        best_3_avg = np.mean(sorted(all_scores)[:3]) if len(all_scores) >= 3 else np.mean(all_scores)
        best_5_avg = np.mean(sorted(all_scores)[:5]) if len(all_scores) >= 5 else np.mean(all_scores)
        
        progression_stats[player_id] = {
            'player_name': player_name,
            'total_rounds': len(all_scores),
            'all_scores': all_scores,
            'overall_avg': overall_avg,
            'recent_5_avg': recent_5_avg,
            'recent_3_avg': recent_3_avg,
            'best_3_avg': best_3_avg,
            'best_5_avg': best_5_avg,
            'score_trend': all_scores[-1] - all_scores[0] if len(all_scores) >= 2 else 0,
            'handicap_info': handicap_info
        }
    
    return progression_stats

def identify_handicap_methodology(progression_stats):
    """Try to identify which averaging method the previous system used"""
    
    print("=== HANDICAP METHODOLOGY ANALYSIS ===\n")
    
    # Load our comparison data
    try:
        comparison_df = pd.read_csv('handicap_comparison.csv')
    except FileNotFoundError:
        print("Handicap comparison file not found. Cannot compare methodologies.")
        return
    
    results = []
    
    for player_id, stats in progression_stats.items():
        # Find this player in comparison data
        player_row = comparison_df[comparison_df['player_name'] == stats['player_name']]
        if player_row.empty:
            continue
            
        sheet_handicap = player_row['sheet_handicap'].iloc[0]
        system_handicap = player_row['system_handicap'].iloc[0]
        sheet_avg = player_row['sheet_avg_score'].iloc[0]
        
        # Calculate differences between various averages and sheet average
        overall_diff = abs(stats['overall_avg'] - sheet_avg)
        recent_5_diff = abs(stats['recent_5_avg'] - sheet_avg)
        recent_3_diff = abs(stats['recent_3_avg'] - sheet_avg)
        best_3_diff = abs(stats['best_3_avg'] - sheet_avg)
        best_5_diff = abs(stats['best_5_avg'] - sheet_avg)
        
        results.append({
            'player_name': stats['player_name'],
            'sheet_avg': sheet_avg,
            'sheet_handicap': sheet_handicap,
            'system_handicap': system_handicap,
            'handicap_diff': abs(sheet_handicap - system_handicap),
            'total_rounds': stats['total_rounds'],
            'overall_avg': stats['overall_avg'],
            'recent_5_avg': stats['recent_5_avg'],
            'recent_3_avg': stats['recent_3_avg'],
            'best_3_avg': stats['best_3_avg'],
            'best_5_avg': stats['best_5_avg'],
            'overall_diff': overall_diff,
            'recent_5_diff': recent_5_diff,
            'recent_3_diff': recent_3_diff,
            'best_3_diff': best_3_diff,
            'best_5_diff': best_5_diff
        })
    
    # Convert to DataFrame for analysis
    analysis_df = pd.DataFrame(results)
    
    if analysis_df.empty:
        print("No matching players found for analysis.")
        return
    
    print("AVERAGE METHODOLOGY COMPARISON:")
    print("=" * 50)
    print(f"Overall Average - Mean Difference: {analysis_df['overall_diff'].mean():.3f}")
    print(f"Recent 5 Average - Mean Difference: {analysis_df['recent_5_diff'].mean():.3f}")
    print(f"Recent 3 Average - Mean Difference: {analysis_df['recent_3_diff'].mean():.3f}")
    print(f"Best 3 Average - Mean Difference: {analysis_df['best_3_diff'].mean():.3f}")
    print(f"Best 5 Average - Mean Difference: {analysis_df['best_5_diff'].mean():.3f}")
    
    # Find the methodology with smallest average difference
    methodologies = {
        'Overall Average': analysis_df['overall_diff'].mean(),
        'Recent 5 Rounds': analysis_df['recent_5_diff'].mean(),
        'Recent 3 Rounds': analysis_df['recent_3_diff'].mean(),
        'Best 3 Rounds': analysis_df['best_3_diff'].mean(),
        'Best 5 Rounds': analysis_df['best_5_diff'].mean()
    }
    
    best_method = min(methodologies, key=methodologies.get)
    print(f"\nLIKELY METHODOLOGY: {best_method}")
    print(f"Smallest average difference: {methodologies[best_method]:.3f}")
    
    # Detailed analysis for players with large discrepancies
    print("\n=== DETAILED ANALYSIS FOR PLAYERS WITH LARGE DISCREPANCIES ===")
    large_discrepancy = analysis_df[analysis_df['handicap_diff'] >= 1.5]
    
    for _, row in large_discrepancy.iterrows():
        print(f"\n{row['player_name']}:")
        print(f"  Sheet Handicap: {row['sheet_handicap']:.1f}, System Handicap: {row['system_handicap']:.1f}")
        print(f"  Sheet Average: {row['sheet_avg']:.1f}")
        print(f"  Overall Average: {row['overall_avg']:.1f} (diff: {row['overall_diff']:.1f})")
        print(f"  Recent 5: {row['recent_5_avg']:.1f} (diff: {row['recent_5_diff']:.1f})")
        print(f"  Recent 3: {row['recent_3_avg']:.1f} (diff: {row['recent_3_diff']:.1f})")
        print(f"  Best 3: {row['best_3_avg']:.1f} (diff: {row['best_3_diff']:.1f})")
        print(f"  Best 5: {row['best_5_avg']:.1f} (diff: {row['best_5_diff']:.1f})")
        print(f"  Total Rounds: {row['total_rounds']}")
    
    # Save detailed analysis
    analysis_df.to_csv('detailed_score_analysis.csv', index=False)
    print(f"\nDetailed analysis saved to 'detailed_score_analysis.csv'")
    
    return analysis_df

def main():
    """Main analysis function"""
    print("Loading score entry data...")
    
    # For now, we'll work with sample data structure
    # In a real scenario, we'd load the actual API data
    
    print("This script is ready to analyze score patterns.")
    print("It requires the score_entries_data.json file from the API call.")
    print("\nTo run the full analysis:")
    print("1. Save the API score entries data to 'score_entries_data.json'")
    print("2. Run this script again")
    
    # If data is available, run the analysis
    score_data = load_score_data()
    if score_data:
        player_weeks = analyze_week_progression(score_data)
        progression_stats = calculate_progression_stats(player_weeks)
        analysis_results = identify_handicap_methodology(progression_stats)
        
        print("\nAnalysis complete!")
    else:
        print("\nScore data not available. Please fetch from API first.")

if __name__ == "__main__":
    main()
