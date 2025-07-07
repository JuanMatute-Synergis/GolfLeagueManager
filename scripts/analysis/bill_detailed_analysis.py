#!/usr/bin/env python3
"""
Detailed analysis of Bill Stein's legacy system calculation
Trying to find the exact formula used.
"""

# Bill's actual scores from our system data
bill_scores = {
    1: 43,   # Week 1 (doesn't count for handicap)
    2: 45,   # Week 2 (doesn't count for handicap)  
    3: 40,   # Week 3 (doesn't count for handicap)
    4: 44,   # Week 4 (counts for handicap)
    5: 44,   # Week 5 (counts for handicap)
    6: None, # Week 6 (rainout)
    7: None, # Week 7 (rainout)
    8: 39,   # Week 8 (counts for handicap)
    9: None, # Week 9 (rainout, though our system shows he was absent with score 42)
    10: 37,  # Week 10 (counts for handicap)
    11: 45,  # Week 11 (counts for handicap)
}

# Legacy system progression
legacy_averages = {
    3: 42.30,
    4: 42.54,
    5: 42.87,
    6: 42.87,  # Same as week 5 (rainout)
    7: 42.87,  # Same as week 5 (rainout)
    8: 42.32,
    9: 42.32,  # Same as week 8 (rainout)
    10: 41.65,
    11: 42.02,
}

print("Detailed Analysis of Legacy System Calculation")
print("=" * 60)

# Let's look at the pattern more carefully
print("Week-by-week changes:")
for week in range(4, 12):
    if week in legacy_averages:
        score = bill_scores.get(week)
        avg = legacy_averages[week]
        prev_avg = legacy_averages.get(week-1, "N/A")
        
        if score is not None:
            print(f"Week {week}: Score {score}, Avg {avg:.2f} (prev: {prev_avg})")
        else:
            print(f"Week {week}: Rainout, Avg {avg:.2f} (prev: {prev_avg})")

print("\n" + "=" * 40)
print("Hypothesis: Maybe they track all weeks including non-counting ones?")

# Let's try including weeks 1-3 in the calculation
print("\nIf they include ALL weeks (1-11) with some weighting:")

# Maybe they use: (initial_avg * weight + sum_of_all_actual_scores) / (weight + count_of_all_scores)
all_scores = [43, 45, 40, 44, 44, 39, 37, 45]  # Weeks 1,2,3,4,5,8,10,11
print(f"All actual scores: {all_scores}")
print(f"Sum of all scores: {sum(all_scores)}")
print(f"Simple average of all scores: {sum(all_scores)/len(all_scores):.2f}")

def test_formula_with_all_weeks(initial_avg, weight):
    print(f"\nTesting with initial_avg={initial_avg:.2f}, weight={weight:.1f}:")
    
    # Track cumulative as we go through weeks
    cumulative_sum = 0
    count = 0
    
    for week in range(1, 12):
        if bill_scores[week] is not None:
            cumulative_sum += bill_scores[week]
            count += 1
            
            # Calculate average using formula
            calculated_avg = (initial_avg * weight + cumulative_sum) / (weight + count)
            
            # Check if this week has a known legacy average
            if week in legacy_averages:
                legacy_avg = legacy_averages[week]
                diff = abs(calculated_avg - legacy_avg)
                print(f"  Week {week}: calc={calculated_avg:.2f}, legacy={legacy_avg:.2f}, diff={diff:.3f}")

# Test different parameters
test_formula_with_all_weeks(42.30, 3.0)
test_formula_with_all_weeks(42.30, 2.5)
test_formula_with_all_weeks(42.30, 2.0)

print("\n" + "=" * 40)
print("Alternative: Maybe they use a different persistence mechanism")

# Maybe the initial average "decays" or they use a running weighted average
# where each new score gets weighted against the current average

def test_decay_formula(decay_factor):
    print(f"\nTesting decay formula with factor {decay_factor:.2f}:")
    print("Formula: new_avg = old_avg * decay_factor + new_score * (1 - decay_factor)")
    
    current_avg = 42.30  # Starting from week 3
    
    for week in range(4, 12):
        score = bill_scores[week]
        
        if score is not None:
            # Apply the decay formula
            new_avg = current_avg * decay_factor + score * (1 - decay_factor)
            
            if week in legacy_averages:
                legacy_avg = legacy_averages[week]
                diff = abs(new_avg - legacy_avg)
                print(f"  Week {week}: score={score}, calc={new_avg:.2f}, legacy={legacy_avg:.2f}, diff={diff:.3f}")
                current_avg = new_avg
        else:
            print(f"  Week {week}: rainout, avg stays {current_avg:.2f}")

# Test different decay factors
test_decay_formula(0.85)
test_decay_formula(0.80)
test_decay_formula(0.75)
test_decay_formula(0.70)
