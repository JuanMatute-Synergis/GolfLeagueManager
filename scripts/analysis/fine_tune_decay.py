#!/usr/bin/env python3
"""
Fine-tuning the decay factor to match legacy system exactly
"""

# Legacy system progression
legacy_averages = {
    3: 42.30,
    4: 42.54,
    5: 42.87,
    8: 42.32,
    10: 41.65,
    11: 42.02,
}

bill_scores = {4: 44, 5: 44, 8: 39, 10: 37, 11: 45}

def test_fine_tuned_decay(decay_factor):
    current_avg = 42.30  # Starting from week 3
    total_diff = 0
    count = 0
    
    print(f"Decay factor: {decay_factor:.3f}")
    
    for week in [4, 5, 8, 10, 11]:
        score = bill_scores[week]
        new_avg = current_avg * decay_factor + score * (1 - decay_factor)
        legacy_avg = legacy_averages[week]
        diff = abs(new_avg - legacy_avg)
        total_diff += diff
        count += 1
        
        print(f"  Week {week}: calc={new_avg:.3f}, legacy={legacy_avg:.3f}, diff={diff:.3f}")
        current_avg = new_avg
    
    avg_diff = total_diff / count
    print(f"  Average difference: {avg_diff:.3f}")
    return avg_diff

print("Fine-tuning decay factor:")
print("=" * 40)

# Test factors around 0.85
factors = [0.840, 0.845, 0.850, 0.855, 0.860, 0.865, 0.870]
best_factor = 0.85
best_diff = float('inf')

for factor in factors:
    diff = test_fine_tuned_decay(factor)
    if diff < best_diff:
        best_diff = diff
        best_factor = factor
    print()

print(f"Best decay factor: {best_factor:.3f} with average difference: {best_diff:.3f}")

print("\n" + "=" * 50)
print("CONCLUSION:")
print(f"The legacy system uses a momentum-based formula:")
print(f"new_average = old_average × {best_factor:.3f} + new_score × {1-best_factor:.3f}")
print()
print("This is fundamentally different from our current system which uses:")
print("- Initial average as baseline")
print("- Phantom scores for non-counting weeks") 
print("- Simple averaging with persistent initial value")
print()
print("The legacy system gives much more weight to recent averages")
print("(persistence/momentum) rather than maintaining a baseline.")
