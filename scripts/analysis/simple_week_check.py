#!/usr/bin/env python3
"""
Simple week check for Bill Stein's legacy averages
Using the momentum/decay formula: new_average = old_average × decay_factor + new_score × (1 - decay_factor)
"""

def calculate_with_decay(scores_and_weeks, decay_factor):
    """Calculate running averages using decay factor"""
    print(f"\nUsing decay factor: {decay_factor}")
    print("Week | Score | Calculated Avg | Legacy Avg | Difference | Notes")
    print("-" * 75)
    
    # Start with initial average
    current_avg = 42.30  # Bill's initial average
    total_diff = 0
    count = 0
    
    for week, score, legacy_avg, notes in scores_and_weeks:
        if "doesn't count" in notes:
            # For non-counting weeks (weeks 1-3), average stays the same (42.30)
            calculated_avg = current_avg
        elif "rainout" in notes:
            # For rainout weeks, apply momentum formula using previous average as the "score"
            current_avg = current_avg * decay_factor + current_avg * (1 - decay_factor)
            # This simplifies to: current_avg = current_avg (stays the same)
            calculated_avg = current_avg
        else:
            # For counting weeks with actual scores, apply momentum formula
            current_avg = current_avg * decay_factor + score * (1 - decay_factor)
            calculated_avg = current_avg
        
        diff = abs(calculated_avg - legacy_avg)
        total_diff += diff
        count += 1
        
        print(f"  {week:2d} | {score:5.1f} | {calculated_avg:10.2f} | {legacy_avg:8.2f} | {diff:8.4f} | {notes}")
    
    avg_diff = total_diff / count if count > 0 else 0
    print(f"\nAverage difference: {avg_diff:.4f}")
    return avg_diff

# Bill Stein's legacy data
# Initial average: 42.30 (weeks 1-3 don't count for average but each count as 42.30)
# Week, Score, Legacy Average, Notes
bill_data = [
    # Week 1-3: don't count for average calculation, each uses 42.30
    (1, 43.0, 42.30, "initial - doesn't count"),
    (2, 45.0, 42.30, "doesn't count"),  
    (3, 40.0, 42.30, "doesn't count"),
    (4, 44.0, 42.54, "first counting week"),
    (5, 44.0, 42.87, "counting"),
    # Weeks 6,7 rainouts - use previous average as "score"
    (6, 42.87, 42.87, "rainout - uses prev avg"),
    (7, 42.87, 42.87, "rainout - uses prev avg"),
    (8, 39.0, 42.32, "counting after rainouts"),
    # Week 9 rainout - use previous average  
    (9, 42.32, 42.32, "rainout - uses prev avg"),
    (10, 37.0, 41.65, "counting"),
    (11, 45.0, 42.02, "counting"),
]

print("Bill Stein Legacy Average Analysis")
print("=================================")

# Test different decay factors
decay_factors = [0.84, 0.845, 0.85, 0.855, 0.86, 0.865, 0.87]

best_factor = None
best_diff = float('inf')

for factor in decay_factors:
    avg_diff = calculate_with_decay(bill_data, factor)
    if avg_diff < best_diff:
        best_diff = avg_diff
        best_factor = factor

print(f"\nBest decay factor: {best_factor} (average difference: {best_diff:.4f})")

# Let's also try some more precise values around the best
if best_factor:
    print("\nFine-tuning around best factor:")
    fine_factors = [
        best_factor - 0.005,
        best_factor - 0.002,
        best_factor - 0.001,
        best_factor,
        best_factor + 0.001,
        best_factor + 0.002,
        best_factor + 0.005
    ]
    
    for factor in fine_factors:
        avg_diff = calculate_with_decay(bill_data, factor)
        if avg_diff < best_diff:
            best_diff = avg_diff
            best_factor = factor
            
    print(f"\nFinal best decay factor: {best_factor} (average difference: {best_diff:.4f})")
