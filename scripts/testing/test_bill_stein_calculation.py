#!/usr/bin/env python3
"""
Test script to verify the corrected legacy weighted average calculation
Based on Bill Stein's example:
- Initial average: 42.30
- Weeks 1,2,3: Don't count for handicap, use initial average (42.30 each)
- Week 4: Actual score 44
- Expected: (42.30 + 42.30 + 42.30 + 42.30 + 44) / 5 = 213.20 / 5 = 42.64
"""

def calculate_legacy_weighted_average(initial_avg, weeks_data):
    """
    Calculate legacy weighted average
    weeks_data is a list of tuples: (week_number, counts_for_handicap, actual_score_or_none)
    The initial average counts as 1 week, then each week adds another week
    """
    # Start with initial average counting as 1 week
    total_score = initial_avg
    total_weeks = 1
    print(f"Initial: {initial_avg} (counts as 1 week)")
    
    for week_num, counts_for_handicap, actual_score in weeks_data:
        total_weeks += 1
        
        if counts_for_handicap and actual_score is not None:
            # Use actual score
            total_score += actual_score
            print(f"Week {week_num}: Actual score {actual_score}")
        else:
            # Use initial average (either doesn't count for handicap or no score)
            total_score += initial_avg
            if counts_for_handicap:
                print(f"Week {week_num}: No score, using initial average {initial_avg}")
            else:
                print(f"Week {week_num}: Doesn't count for handicap, using initial average {initial_avg}")
    
    if total_weeks == 0:
        return initial_avg
    
    average = total_score / total_weeks
    print(f"Total score: {total_score}, Total weeks: {total_weeks}")
    print(f"Average: {total_score} / {total_weeks} = {average:.2f}")
    return round(average, 2)

print("Bill Stein Legacy Weighted Average Test")
print("======================================")

# Bill Stein's data
initial_average = 42.30

# Week data: (week_number, counts_for_handicap, actual_score_or_none)
bill_weeks = [
    (1, False, None),   # Week 1: Doesn't count for handicap
    (2, False, None),   # Week 2: Doesn't count for handicap  
    (3, False, None),   # Week 3: Doesn't count for handicap
    (4, True, 44.0),    # Week 4: Counts for handicap, actual score 44
]

result = calculate_legacy_weighted_average(initial_average, bill_weeks)
print(f"\nFinal calculated average: {result}")
print(f"Expected: 42.64")
print(f"Match: {'✅ YES' if abs(result - 42.64) < 0.01 else '❌ NO'}")

print("\n" + "="*50)
print("Kevin Kelhart Example (if we had his data)")
print("="*50)

# Example Kevin data (replace with actual when available)
kevin_initial = 40.0
kevin_weeks = [
    (1, False, None),   # Week 1: Doesn't count for handicap
    (2, False, None),   # Week 2: Doesn't count for handicap
    (3, False, None),   # Week 3: Doesn't count for handicap  
    (4, True, 38.0),    # Week 4: Actual score 38
]

kevin_result = calculate_legacy_weighted_average(kevin_initial, kevin_weeks)
print(f"\nKevin's calculated average: {kevin_result}")
print(f"Formula: (40.0 + 40.0 + 40.0 + 40.0 + 38.0) / 5 = 198.0 / 5 = 39.60")
