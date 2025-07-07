#!/usr/bin/env python3
"""
Test the SimpleAverage calculation fix for Kevin's issue
"""

def test_simple_average_fix():
    print("Kevin K. SimpleAverage Calculation Fix Test")
    print("==========================================")
    
    initial_avg = 43.53
    week2_score = 42.0  # Assuming Kevin shot 42 in Week 2
    
    print(f"Initial Average: {initial_avg}")
    print(f"Week 1: Doesn't count for handicap")
    print(f"Week 2: {week2_score} (counts for handicap)")
    
    # OLD (buggy) SimpleAverage calculation
    print(f"\nOLD (buggy) calculation:")
    print(f"  Week 1: scoresForCalculation = [{initial_avg}] (from non-counting week)")
    print(f"  Week 1 result: {initial_avg}")
    print(f"  Week 2: actualScores = [{week2_score}]")
    print(f"  Week 2: currentValidAverage = {week2_score} (BUG: excludes initial)")
    print(f"  Week 2: scoresForCalculation = [{week2_score}, {week2_score}] (BUG)")
    old_avg = (week2_score + week2_score) / 2
    print(f"  Week 2 result: {old_avg} (WRONG!)")
    
    # NEW (fixed) SimpleAverage calculation  
    print(f"\nNEW (fixed) calculation:")
    print(f"  Week 1: No actual scores, return initial average")
    print(f"  Week 1 result: {initial_avg}")
    print(f"  Week 2: actualScores = [{week2_score}]")
    print(f"  Week 2: total = {initial_avg} + {week2_score} = {initial_avg + week2_score}")
    print(f"  Week 2: count = 1 + 1 = 2")
    new_avg = (initial_avg + week2_score) / 2
    print(f"  Week 2 result: {new_avg}")
    
    print(f"\nComparison:")
    print(f"  Kevin's actual Week 1 to Week 2 change: 43.53 → 43.22")
    print(f"  OLD buggy calculation would give: 43.53 → {old_avg}")
    print(f"  NEW fixed calculation would give: 43.53 → {new_avg}")
    
    # Check if our fix matches Kevin's actual data
    if abs(new_avg - 43.22) < 0.1:
        print(f"  ✅ NEW calculation matches Kevin's data!")
    else:
        print(f"  ❌ NEW calculation doesn't match. Kevin's Week 2 score might be different.")
        
        # Reverse engineer what Week 2 score would give 43.22
        # (43.53 + week2_score) / 2 = 43.22
        # 43.53 + week2_score = 86.44
        # week2_score = 86.44 - 43.53
        correct_week2_score = (43.22 * 2) - 43.53
        print(f"  Kevin's actual Week 2 score was likely: {correct_week2_score:.2f}")

def test_legacy_vs_simple():
    print(f"\n\nLegacy vs Simple Average Comparison")
    print(f"===================================")
    
    initial_avg = 43.53
    week2_score = 42.91  # The score that would give 43.22 in SimpleAverage
    
    print(f"If Kevin's Week 2 score was {week2_score}:")
    
    # SimpleAverage calculation
    simple_avg = (initial_avg + week2_score) / 2
    print(f"  SimpleAverage: ({initial_avg} + {week2_score}) / 2 = {simple_avg:.2f}")
    
    # LegacyWeightedAverage calculation
    legacy_avg = (initial_avg + initial_avg + week2_score) / 3
    print(f"  LegacyWeightedAverage: ({initial_avg} + {initial_avg} + {week2_score}) / 3 = {legacy_avg:.2f}")
    
    print(f"\nThis suggests Kevin's calculation is using SimpleAverage method, not LegacyWeightedAverage")

if __name__ == "__main__":
    test_simple_average_fix()
    test_legacy_vs_simple()
