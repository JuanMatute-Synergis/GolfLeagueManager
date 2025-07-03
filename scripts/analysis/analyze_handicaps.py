#!/usr/bin/env python3
import json
import pandas as pd
from collections import defaultdict

def analyze_handicap_patterns():
    print("Loading score entries data...")
    
    # Load score data
    with open('score_entries_data.json', 'r') as f:
        score_data = json.load(f)
    
    print(f"Loaded {len(score_data)} score entries")
    
    # Group by player
    players = defaultdict(list)
    for entry in score_data:
        if entry.get('score', 0) > 0:  # Only include actual scores
            players[entry['playerId']].append(entry)
    
    print(f"Found {len(players)} players with scores")
    
    # Load comparison data
    try:
        comparison_df = pd.read_csv('handicap_comparison.csv')
        print(f"Loaded comparison data for {len(comparison_df)} players")
    except FileNotFoundError:
        print("Handicap comparison file not found!")
        return
    
    # Analyze each player
    results = []
    
    for player_id, entries in players.items():
        if len(entries) < 3:  # Need at least 3 scores
            continue
            
        # Get player info from first entry
        player_info = entries[0]['player']
        player_name = f"{player_info['firstName']} {player_info['lastName']}"
        
        # Find this player in comparison data
        player_row = comparison_df[comparison_df['player_name'] == player_name]
        if player_row.empty:
            continue
        
        sheet_handicap = player_row['sheet_handicap'].iloc[0]
        system_handicap = player_row['system_handicap'].iloc[0]
        sheet_avg = player_row['sheet_avg_score'].iloc[0]
        
        # Extract scores and sort by entry order
        scores = [entry['score'] for entry in entries]
        
        # Calculate different averages
        overall_avg = sum(scores) / len(scores)
        recent_5_avg = sum(scores[-5:]) / min(5, len(scores))
        recent_3_avg = sum(scores[-3:]) / min(3, len(scores))
        best_3_avg = sum(sorted(scores)[:3]) / min(3, len(scores))
        best_5_avg = sum(sorted(scores)[:5]) / min(5, len(scores))
        
        # Calculate differences from sheet average
        overall_diff = abs(overall_avg - sheet_avg)
        recent_5_diff = abs(recent_5_avg - sheet_avg)
        recent_3_diff = abs(recent_3_avg - sheet_avg)
        best_3_diff = abs(best_3_avg - sheet_avg)
        best_5_diff = abs(best_5_avg - sheet_avg)
        
        results.append({
            'player_name': player_name,
            'num_scores': len(scores),
            'sheet_avg': sheet_avg,
            'sheet_handicap': sheet_handicap,
            'system_handicap': system_handicap,
            'handicap_diff': abs(sheet_handicap - system_handicap),
            'overall_avg': overall_avg,
            'recent_5_avg': recent_5_avg,
            'recent_3_avg': recent_3_avg,
            'best_3_avg': best_3_avg,
            'best_5_avg': best_5_avg,
            'overall_diff': overall_diff,
            'recent_5_diff': recent_5_diff,
            'recent_3_diff': recent_3_diff,
            'best_3_diff': best_3_diff,
            'best_5_diff': best_5_diff,
            'scores': scores
        })
    
    print(f"Analyzed {len(results)} players")
    
    if not results:
        print("No results to analyze!")
        return
    
    # Calculate methodology comparison
    print("\n=== HANDICAP METHODOLOGY ANALYSIS ===")
    print("=" * 50)
    
    overall_diffs = [r['overall_diff'] for r in results]
    recent_5_diffs = [r['recent_5_diff'] for r in results]
    recent_3_diffs = [r['recent_3_diff'] for r in results]
    best_3_diffs = [r['best_3_diff'] for r in results]
    best_5_diffs = [r['best_5_diff'] for r in results]
    
    methodologies = {
        'Overall Average': sum(overall_diffs) / len(overall_diffs),
        'Recent 5 Rounds': sum(recent_5_diffs) / len(recent_5_diffs),
        'Recent 3 Rounds': sum(recent_3_diffs) / len(recent_3_diffs),
        'Best 3 Rounds': sum(best_3_diffs) / len(best_3_diffs),
        'Best 5 Rounds': sum(best_5_diffs) / len(best_5_diffs)
    }
    
    print("AVERAGE METHODOLOGY COMPARISON:")
    for method, avg_diff in methodologies.items():
        print(f"{method:<20} - Mean Difference: {avg_diff:.3f}")
    
    best_method = min(methodologies, key=methodologies.get)
    print(f"\nLIKELY METHODOLOGY: {best_method}")
    print(f"Smallest average difference: {methodologies[best_method]:.3f}")
    
    # Show players with large discrepancies
    print("\n=== PLAYERS WITH LARGE HANDICAP DISCREPANCIES ===")
    large_discrepancies = [r for r in results if r['handicap_diff'] >= 1.5]
    
    for player in sorted(large_discrepancies, key=lambda x: x['handicap_diff'], reverse=True):
        print(f"\n{player['player_name']}:")
        print(f"  Sheet Handicap: {player['sheet_handicap']:.1f}, System Handicap: {player['system_handicap']:.1f} (diff: {player['handicap_diff']:.1f})")
        print(f"  Sheet Average: {player['sheet_avg']:.1f}")
        print(f"  Number of Scores: {player['num_scores']}")
        print(f"  Overall Average: {player['overall_avg']:.1f} (diff: {player['overall_diff']:.1f})")
        print(f"  Recent 5: {player['recent_5_avg']:.1f} (diff: {player['recent_5_diff']:.1f})")
        print(f"  Recent 3: {player['recent_3_avg']:.1f} (diff: {player['recent_3_diff']:.1f})")
        print(f"  Best 3: {player['best_3_avg']:.1f} (diff: {player['best_3_diff']:.1f})")
        print(f"  Best 5: {player['best_5_avg']:.1f} (diff: {player['best_5_diff']:.1f})")
        print(f"  All Scores: {player['scores']}")
    
    # Save detailed results
    df = pd.DataFrame(results)
    df.to_csv('detailed_score_analysis.csv', index=False)
    print(f"\nDetailed analysis saved to 'detailed_score_analysis.csv'")

if __name__ == "__main__":
    analyze_handicap_patterns()
