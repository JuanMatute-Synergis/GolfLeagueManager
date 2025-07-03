import json
import pandas as pd
import sys

print("Starting handicap analysis...", flush=True)
sys.stdout.flush()

# Load data
with open('score_entries_data.json', 'r') as f:
    score_data = json.load(f)

comparison_df = pd.read_csv('handicap_comparison.csv')

print(f"Loaded {len(score_data)} score entries", flush=True)
print(f"Loaded comparison data for {len(comparison_df)} players", flush=True)
sys.stdout.flush()

# Group scores by player
player_scores = {}
for entry in score_data:
    if entry.get('score', 0) > 0:  # Only include actual scores
        player_name = f"{entry['player']['firstName']} {entry['player']['lastName']}"
        if player_name not in player_scores:
            player_scores[player_name] = []
        player_scores[player_name].append(entry['score'])

print(f"Found scores for {len(player_scores)} players")

# Analyze methodology
results = []
for player_name, scores in player_scores.items():
    if len(scores) < 3:
        continue
        
    # Find in comparison data
    player_row = comparison_df[comparison_df['Player Name'] == player_name]
    if player_row.empty:
        continue
        
    sheet_avg = player_row['Sheet Avg Score'].iloc[0]
    sheet_handicap = player_row['Sheet Handicap'].iloc[0]
    system_handicap = player_row['System Current Handicap'].iloc[0]
    
    # Calculate different averages
    overall_avg = sum(scores) / len(scores)
    recent_5_avg = sum(scores[-5:]) / min(5, len(scores))
    recent_3_avg = sum(scores[-3:]) / min(3, len(scores))
    best_3_avg = sum(sorted(scores)[:3]) / min(3, len(scores))
    best_5_avg = sum(sorted(scores)[:5]) / min(5, len(scores))
    
    # Calculate differences
    overall_diff = abs(overall_avg - sheet_avg)
    recent_5_diff = abs(recent_5_avg - sheet_avg)
    recent_3_diff = abs(recent_3_avg - sheet_avg)
    best_3_diff = abs(best_3_avg - sheet_avg)
    best_5_diff = abs(best_5_avg - sheet_avg)
    
    results.append({
        'player': player_name,
        'scores': scores,
        'sheet_avg': sheet_avg,
        'sheet_handicap': sheet_handicap,
        'system_handicap': system_handicap,
        'overall_avg': overall_avg,
        'recent_5_avg': recent_5_avg,
        'recent_3_avg': recent_3_avg,
        'best_3_avg': best_3_avg,
        'best_5_avg': best_5_avg,
        'overall_diff': overall_diff,
        'recent_5_diff': recent_5_diff,
        'recent_3_diff': recent_3_diff,
        'best_3_diff': best_3_diff,
        'best_5_diff': best_5_diff
    })

print(f"Analyzed {len(results)} players")

# Calculate average differences for each methodology
if results:
    overall_avg_diff = sum(r['overall_diff'] for r in results) / len(results)
    recent_5_avg_diff = sum(r['recent_5_diff'] for r in results) / len(results)
    recent_3_avg_diff = sum(r['recent_3_diff'] for r in results) / len(results)
    best_3_avg_diff = sum(r['best_3_diff'] for r in results) / len(results)
    best_5_avg_diff = sum(r['best_5_diff'] for r in results) / len(results)
    
    print("\n=== METHODOLOGY COMPARISON ===")
    print(f"Overall Average:  {overall_avg_diff:.3f}")
    print(f"Recent 5 Rounds:  {recent_5_avg_diff:.3f}")
    print(f"Recent 3 Rounds:  {recent_3_avg_diff:.3f}")
    print(f"Best 3 Rounds:    {best_3_avg_diff:.3f}")
    print(f"Best 5 Rounds:    {best_5_avg_diff:.3f}")
    
    # Find best methodology
    methodologies = {
        'Overall Average': overall_avg_diff,
        'Recent 5 Rounds': recent_5_avg_diff,
        'Recent 3 Rounds': recent_3_avg_diff,
        'Best 3 Rounds': best_3_avg_diff,
        'Best 5 Rounds': best_5_avg_diff
    }
    
    best_method = min(methodologies, key=methodologies.get)
    print(f"\nLIKELY METHODOLOGY: {best_method}")
    print(f"Smallest difference: {methodologies[best_method]:.3f}")
    
    # Show players with large handicap differences
    print("\n=== PLAYERS WITH LARGE HANDICAP DISCREPANCIES ===")
    large_diff_players = [r for r in results if abs(r['sheet_handicap'] - r['system_handicap']) >= 1.5]
    
    for player in sorted(large_diff_players, key=lambda x: abs(x['sheet_handicap'] - x['system_handicap']), reverse=True):
        handicap_diff = abs(player['sheet_handicap'] - player['system_handicap'])
        print(f"\n{player['player']}:")
        print(f"  Sheet Handicap: {player['sheet_handicap']}, System: {player['system_handicap']} (diff: {handicap_diff})")
        print(f"  Sheet Avg: {player['sheet_avg']:.1f}")
        print(f"  Overall Avg: {player['overall_avg']:.1f} (diff: {player['overall_diff']:.1f})")
        print(f"  Recent 5: {player['recent_5_avg']:.1f} (diff: {player['recent_5_diff']:.1f})")
        print(f"  Recent 3: {player['recent_3_avg']:.1f} (diff: {player['recent_3_diff']:.1f})")
        print(f"  Best 3: {player['best_3_avg']:.1f} (diff: {player['best_3_diff']:.1f})")
        print(f"  Best 5: {player['best_5_avg']:.1f} (diff: {player['best_5_diff']:.1f})")
        print(f"  All Scores ({len(player['scores'])}): {player['scores']}")

print("\nAnalysis complete!")
