#!/usr/bin/env python3
"""
Analysis script for Kevin Kelhart up to week 4
Checking if the legacy weighted average calculation (initial weight = 1) is working correctly
"""

def calculate_weighted_average(scores_and_weeks, initial_avg, initial_weight):
    """Calculate weighted average using: (initial_avg * initial_weight + sum_of_scores) / (initial_weight + number_of_scores)"""
    results = []
    
    for week, score, legacy_avg in scores_and_weeks:
        # Get all scores up to this week
        scores_up_to_week = [s for w, s, _ in scores_and_weeks if w <= week]
        
        # Calculate weighted average
        sum_of_scores = sum(scores_up_to_week)
        number_of_scores = len(scores_up_to_week)
        
        calculated_avg = (initial_avg * initial_weight + sum_of_scores) / (initial_weight + number_of_scores)
        
        diff = abs(calculated_avg - legacy_avg)
        results.append((week, score, calculated_avg, legacy_avg, diff))
        
        print(f"Week {week}: Score={score}, Calculated={calculated_avg:.2f}, Legacy={legacy_avg:.2f}, Diff={diff:.2f}")
        print(f"  Formula: ({initial_avg} * {initial_weight} + {sum_of_scores}) / ({initial_weight} + {number_of_scores}) = {calculated_avg:.2f}")
    
    return results

# We need to find Kevin Kelhart's data first
# Let me check what data sources we have available

print("Kevin Kelhart Analysis - Legacy Weighted Average Check")
print("====================================================")
print("Looking for Kevin Kelhart's data...")

# Check if we can access the database or Excel data
import os
import sys

# Check what data files we have
data_files = []
for file in os.listdir('.'):
    if file.endswith('.csv') or file.endswith('.xls') or file.endswith('.xlsx'):
        data_files.append(file)

print(f"Available data files: {data_files}")

# Let's try to read the Excel file that was mentioned
try:
    import pandas as pd
    
    excel_file = "2025 Playa Scores data copy 8-28-24.xls"
    if os.path.exists(excel_file):
        print(f"Reading {excel_file}...")
        
        # Try to read different sheets
        try:
            xls = pd.ExcelFile(excel_file)
            print(f"Available sheets: {xls.sheet_names}")
            
            # Look for Kevin Kelhart in each sheet
            for sheet_name in xls.sheet_names:
                try:
                    df = pd.read_excel(excel_file, sheet_name=sheet_name)
                    print(f"\nSheet: {sheet_name}")
                    print(f"Columns: {list(df.columns)}")
                    
                    # Look for Kevin in the data
                    if 'Player' in df.columns or 'Name' in df.columns:
                        player_col = 'Player' if 'Player' in df.columns else 'Name'
                        kevin_data = df[df[player_col].str.contains('Kevin', case=False, na=False)]
                        if not kevin_data.empty:
                            print(f"Found Kevin data in {sheet_name}:")
                            print(kevin_data.head())
                    elif any('kevin' in str(col).lower() for col in df.columns):
                        print(f"Found Kevin-related columns: {[col for col in df.columns if 'kevin' in str(col).lower()]}")
                        
                except Exception as e:
                    print(f"Error reading sheet {sheet_name}: {e}")
                    
        except Exception as e:
            print(f"Error reading Excel file: {e}")
    else:
        print(f"Excel file {excel_file} not found")
        
except ImportError:
    print("pandas not available, cannot read Excel files")

# If we can't find the data automatically, let's provide some sample data for Kevin
# and show what the analysis would look like
print("\nAssuming Kevin Kelhart has some scores, here's how the analysis would work:")
print("(Please provide Kevin's actual data: initial average and scores for weeks 1-4)")

# Example analysis with hypothetical data
kevin_example = [
    # (week, score, legacy_average_from_old_system)
    (1, 38.0, 39.5),  # Example: Week 1, score 38, legacy system showed 39.5
    (2, 42.0, 40.0),  # Week 2, score 42, legacy system showed 40.0  
    (3, 40.0, 40.0),  # Week 3, score 40, legacy system showed 40.0
    (4, 36.0, 39.5),  # Week 4, score 36, legacy system showed 39.5
]

print("\nExample Analysis (replace with Kevin's actual data):")
print("Initial Average: 40.0, Initial Weight: 1")
calculate_weighted_average(kevin_example, 40.0, 1)
