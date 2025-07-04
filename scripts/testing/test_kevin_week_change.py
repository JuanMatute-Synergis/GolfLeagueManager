#!/usr/bin/env python3
"""
Test what happens to Kevin's average from Week 1 to Week 2
"""

def test_kevin_week1_to_week2():
    print("Kevin K. Week 1 to Week 2 Average Change Test")
    print("=============================================")
    
    # Kevin K. #1 data
    initial_avg = 43.52
    week1_score = 52  # doesn't count for handicap
    week2_score = 45  # let's assume this counts for handicap
    
    print(f"Kevin K. #1:")
    print(f"Initial Average: {initial_avg}")
    print(f"Week 1 Score: {week1_score} (doesn't count for handicap)")
    print(f"Week 2 Score: {week2_score} (counts for handicap)")
    
    # Week 1 calculation (legacy weighted average)
    print(f"\nWeek 1 Calculation:")
    print(f"  Initial: {initial_avg} (counts as 1 week)")
    print(f"  Week 1: {initial_avg} (doesn't count, use initial)")
    week1_total = initial_avg + initial_avg
    week1_count = 2
    week1_avg = week1_total / week1_count
    print(f"  Total: ({initial_avg} + {initial_avg}) / 2 = {week1_avg}")
    
    # Week 2 calculation (legacy weighted average)
    print(f"\nWeek 2 Calculation:")
    print(f"  Initial: {initial_avg} (counts as 1 week)")
    print(f"  Week 1: {initial_avg} (doesn't count, use initial)")
    print(f"  Week 2: {week2_score} (counts for handicap, use actual)")
    week2_total = initial_avg + initial_avg + week2_score
    week2_count = 3
    week2_avg = week2_total / week2_count
    print(f"  Total: ({initial_avg} + {initial_avg} + {week2_score}) / 3 = {week2_avg:.2f}")
    
    print(f"\nChange from Week 1 to Week 2:")
    print(f"  Week 1 Average: {week1_avg:.2f}")
    print(f"  Week 2 Average: {week2_avg:.2f}")
    print(f"  Change: {week2_avg - week1_avg:.2f}")
    
    if abs(week2_avg - week1_avg) > 0.5:
        print(f"  🚨 SIGNIFICANT CHANGE! This could be the issue.")
    else:
        print(f"  ✅ Normal change expected.")

def test_potential_bug():
    print(f"\n\nPotential Bug Analysis")
    print(f"=====================")
    
    # If there's a bug where the calculation is inconsistent between weeks
    initial_avg = 43.52
    
    print(f"Scenario: Calculation bug between Week 1 and Week 2")
    print(f"Initial Average: {initial_avg}")
    
    # Bug 1: Week 1 calculation is wrong
    print(f"\nBug 1: Week 1 uses wrong calculation")
    print(f"  Wrong Week 1: Uses gross score instead of initial average")
    print(f"  Week 1 shows: 52 (gross score) instead of {initial_avg}")
    
    # Bug 2: Inconsistent method between weeks
    print(f"\nBug 2: Method changes between weeks")
    print(f"  Week 1: Uses SimpleAverage method")
    print(f"  Week 2: Uses LegacyWeightedAverage method")
    print(f"  This would cause unexpected changes")
    
    # Bug 3: Rounding issues
    print(f"\nBug 3: Rounding issues in calculation")
    print(f"  Math.Round(initialAverage, 2) in the middle of calculation")
    print(f"  Could cause cumulative rounding errors")

if __name__ == "__main__":
    test_kevin_week1_to_week2()
    test_potential_bug()
