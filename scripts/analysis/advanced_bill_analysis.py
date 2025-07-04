#!/usr/bin/env python3
"""
Advanced analysis to find Bill Stein's initial average and decay factor
"""

def calculate_with_initial_avg_and_decay(scores_and_weeks, initial_avg, decay_factor):
    """Calculate running averages using initial average and decay factor"""
    current_avg = initial_avg
    total_diff = 0
    count = 0
    results = []
    
    for week, score, legacy_avg in scores_and_weeks:
        # Apply momentum formula
        current_avg = current_avg * decay_factor + score * (1 - decay_factor)
        
        diff = abs(current_avg - legacy_avg)
        total_diff += diff
        count += 1
        
        results.append((week, score, current_avg, legacy_avg, diff))
    
    avg_diff = total_diff / count if count > 0 else 0
    return avg_diff, results

# Bill Stein's legacy data
bill_data = [
    (3, 44.0, 42.30),  
    (4, 44.0, 42.54),  
    (5, 44.0, 42.87),
    (8, 39.0, 42.32),  
    (10, 37.0, 41.65), 
    (11, 45.0, 42.02), 
]

print("Bill Stein Advanced Analysis")
print("============================")

best_combo = None
best_diff = float('inf')

# Test various combinations of initial average and decay factor
initial_averages = [38.0, 39.0, 40.0, 41.0, 42.0, 43.0]
decay_factors = [0.80, 0.82, 0.83, 0.835, 0.84, 0.845, 0.85, 0.86, 0.88, 0.90]

for initial_avg in initial_averages:
    for decay_factor in decay_factors:
        avg_diff, results = calculate_with_initial_avg_and_decay(bill_data, initial_avg, decay_factor)
        
        if avg_diff < best_diff:
            best_diff = avg_diff
            best_combo = (initial_avg, decay_factor, results)

print(f"Best combination found:")
print(f"Initial Average: {best_combo[0]}")
print(f"Decay Factor: {best_combo[1]}")
print(f"Average Difference: {best_diff:.4f}")
print()

print("Week | Score | Calculated Avg | Legacy Avg | Difference")
print("-" * 60)
for week, score, calc_avg, legacy_avg, diff in best_combo[2]:
    print(f"  {week:2d} | {score:5.1f} | {calc_avg:10.2f} | {legacy_avg:8.2f} | {diff:8.4f}")

# Fine-tune around the best combination
print("\nFine-tuning...")
best_initial, best_decay, _ = best_combo

fine_initials = [best_initial - 0.5, best_initial - 0.2, best_initial, best_initial + 0.2, best_initial + 0.5]
fine_decays = [best_decay - 0.01, best_decay - 0.005, best_decay, best_decay + 0.005, best_decay + 0.01]

for initial_avg in fine_initials:
    for decay_factor in fine_decays:
        avg_diff, results = calculate_with_initial_avg_and_decay(bill_data, initial_avg, decay_factor)
        
        if avg_diff < best_diff:
            best_diff = avg_diff
            best_combo = (initial_avg, decay_factor, results)

print(f"\nFinal best combination:")
print(f"Initial Average: {best_combo[0]}")
print(f"Decay Factor: {best_combo[1]}")
print(f"Average Difference: {best_diff:.4f}")

# Now let's work backwards from week 3 to see what the initial average should be
print("\n" + "="*60)
print("WORKING BACKWARDS FROM WEEK 3")
print("="*60)

# If week 3 legacy average is 42.30 and he shot 44, what was his average before?
# 42.30 = prev_avg * decay + 44 * (1 - decay)
# Solving for prev_avg: prev_avg = (42.30 - 44 * (1 - decay)) / decay

for decay in [0.80, 0.82, 0.835, 0.84, 0.85, 0.86, 0.88, 0.90]:
    prev_avg = (42.30 - 44 * (1 - decay)) / decay
    print(f"Decay {decay}: Previous average would be {prev_avg:.2f}")
    
    # Test this combination
    avg_diff, results = calculate_with_initial_avg_and_decay(bill_data, prev_avg, decay)
    print(f"  → Average difference with this combo: {avg_diff:.4f}")
    print()
