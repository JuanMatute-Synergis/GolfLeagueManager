#!/usr/bin/env python3
"""
Test Kevin K's average calculation issue
Week 1: countsForHandicap = false, grossScore = 52 or 47
Expected: average should be close to initial average, not the gross score
"""

def test_week1_calculation():
    print("Kevin K. Average Calculation Test - Week 1")
    print("=========================================")
    
    # Scenario 1: Kevin K. #1
    print("\nKevin K. #1:")
    print("- Gross Score Week 1: 52")
    print("- Average Score: 43.52")
    print("- Week 1 countsForHandicap: false")
    
    # If Week 1 doesn't count for handicap, the average should be the initial average
    # But we're seeing 43.52, which suggests the initial average might be around 43.52
    
    # Let's reverse engineer what initial average would give us 43.52 as the result
    # In legacy weighted average with Week 1 not counting for handicap:
    # Result = (initial_avg + initial_avg) / 2 = initial_avg
    # So initial_avg should be 43.52
    
    print("- If initial average = 43.52, then Week 1 average should be 43.52 ✓")
    print("- But why is initial average 43.52 when gross score is 52?")
    
    print("\nKevin K. #2:")
    print("- Gross Score Week 1: 47") 
    print("- Average Score: 48.22")
    print("- Week 1 countsForHandicap: false")
    print("- If initial average = 48.22, then Week 1 average should be 48.22 ✓")
    print("- But why is initial average 48.22 when gross score is 47?")
    
    print("\nPossible Issues:")
    print("1. Initial averages are set incorrectly in the database")
    print("2. The calculation is using a different method than expected")
    print("3. There's a bug in the legacy weighted average calculation")
    print("4. The system is using SimpleAverage instead of LegacyWeightedAverage")

def test_simple_vs_legacy():
    print("\n\nSimple vs Legacy Average Test")
    print("============================")
    
    initial_avg = 40.0  # Example initial average
    week1_score = 52    # Kevin's actual score
    
    print(f"Initial Average: {initial_avg}")
    print(f"Week 1 Score: {week1_score} (doesn't count for handicap)")
    
    # Simple Average method
    print(f"\nSimple Average method:")
    print(f"  Week 1 doesn't count for handicap, so uses initial average")
    print(f"  Result: {initial_avg}")
    
    # Legacy Weighted Average method  
    print(f"\nLegacy Weighted Average method:")
    print(f"  Initial: {initial_avg} (counts as 1 week)")
    print(f"  Week 1: {initial_avg} (doesn't count for handicap, use initial)")
    print(f"  Total: ({initial_avg} + {initial_avg}) / 2 = {initial_avg}")
    print(f"  Result: {initial_avg}")
    
    print(f"\nBoth methods should give {initial_avg}, not {week1_score}")
    print(f"The fact that we see 43.52 and 48.22 suggests these are the initial averages")

if __name__ == "__main__":
    test_week1_calculation()
    test_simple_vs_legacy()
