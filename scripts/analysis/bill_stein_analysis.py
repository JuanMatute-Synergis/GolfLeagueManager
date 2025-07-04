#!/usr/bin/env python3
"""
Analysis of Bill Stein's average progression in the legacy system
to reverse-engineer their calculation method.
"""

# Bill Stein's progression in the legacy system
legacy_data = [
    {"week": 3, "score": 40, "average": 42.30, "notes": "week 3 average"},
    {"week": 4, "score": 44, "average": 42.54, "notes": "shot 44"},
    {"week": 5, "score": 44, "average": 42.87, "notes": "shot 44"},
    {"week": 6, "score": None, "average": 42.87, "notes": "rainout"},
    {"week": 7, "score": None, "average": 42.87, "notes": "rainout"},
    {"week": 8, "score": 39, "average": 42.32, "notes": "shot 39"},
    {"week": 9, "score": None, "average": 42.32, "notes": "rainout"},
    {"week": 10, "score": 37, "average": 41.65, "notes": "shot 37"},
    {"week": 11, "score": 45, "average": 42.02, "notes": "shot 45"},
]

print("Bill Stein Legacy System Analysis")
print("=" * 50)

# Let's analyze the changes
previous_avg = None
for entry in legacy_data:
    week = entry["week"]
    score = entry["score"]
    avg = entry["average"]
    notes = entry["notes"]
    
    if previous_avg is not None:
        change = avg - previous_avg
        print(f"Week {week}: Score={score}, Avg={avg:.2f}, Change={change:+.2f}, {notes}")
    else:
        print(f"Week {week}: Score={score}, Avg={avg:.2f}, {notes}")
    
    previous_avg = avg

print("\n" + "=" * 50)
print("Analysis:")

# Let's try to reverse engineer the formula
print("\nTrying to reverse-engineer the calculation:")

# From the data, it appears they might be using a weighted average
# Let's see if we can find the pattern

# Week 4: 42.30 -> 42.54 with score 44
# If it's a simple weighted average: (initial_weight * 42.30 + 44) / (initial_weight + 1) = 42.54
# Solving: initial_weight * 42.30 + 44 = 42.54 * (initial_weight + 1)
# initial_weight * 42.30 + 44 = 42.54 * initial_weight + 42.54
# 44 - 42.54 = 42.54 * initial_weight - 42.30 * initial_weight
# 1.46 = initial_weight * (42.54 - 42.30)
# 1.46 = initial_weight * 0.24
# initial_weight = 1.46 / 0.24 = 6.083...

print("Week 4 analysis:")
print(f"  Previous avg: 42.30, New score: 44, New avg: 42.54")
initial_weight = (44 - 42.54) / (42.54 - 42.30)
print(f"  Calculated initial weight: {initial_weight:.3f}")

# Let's verify this with Week 5
print("\nWeek 5 verification:")
print(f"  Previous avg: 42.54, New score: 44, Expected avg using weight 6.083:")
# (6.083 * 42.54 + 44) / (6.083 + 1) = ?
expected_avg = (6.083 * 42.54 + 44) / (6.083 + 1)
print(f"  Expected: {expected_avg:.2f}, Actual: 42.87")

# Let's try a different approach - maybe it's tracking cumulative scores
print("\n" + "=" * 30)
print("Alternative approach - cumulative tracking:")

# Maybe they track: (initial_average * initial_weight + sum_of_actual_scores) / (initial_weight + count_of_actual_scores)
# Let's assume initial average is something and initial weight is something

# Working backwards from known data points
scores_played = []
averages = []

for entry in legacy_data:
    if entry["score"] is not None:
        scores_played.append(entry["score"])
        averages.append(entry["average"])

print("Actual scores played:", scores_played)
print("Resulting averages:", averages)

# Let's try to find initial values
# Assume formula: (initial_avg * weight + sum_scores) / (weight + count_scores) = current_avg

def test_initial_values(initial_avg, weight):
    print(f"\nTesting initial_avg={initial_avg:.2f}, weight={weight:.1f}:")
    cumulative_scores = 0
    count_scores = 0
    
    for i, score in enumerate([40, 44, 44, 39, 37, 45]):  # All actual scores through week 11
        cumulative_scores += score
        count_scores += 1
        
        calculated_avg = (initial_avg * weight + cumulative_scores) / (weight + count_scores)
        
        # Find corresponding actual average from our data
        score_weeks = [3, 4, 5, 8, 10, 11]
        if i < len(score_weeks):
            actual_week = score_weeks[i]
            actual_avg = next(entry["average"] for entry in legacy_data if entry["week"] == actual_week)
            diff = abs(calculated_avg - actual_avg)
            print(f"  After score {score}: calc={calculated_avg:.2f}, actual={actual_avg:.2f}, diff={diff:.3f}")

# Test some values
test_initial_values(42.30, 6.0)
test_initial_values(42.30, 5.0)
test_initial_values(42.30, 4.0)
test_initial_values(42.25, 5.0)
