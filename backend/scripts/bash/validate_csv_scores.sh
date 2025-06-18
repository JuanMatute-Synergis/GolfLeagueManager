#!/bin/bash

echo "================================================"
echo "Golf League Score Validation - Original vs Corrected CSV"
echo "================================================"

ORIGINAL_CSV="/Users/juanmatute/Downloads/download.csv"
CORRECTED_CSV="/Users/juanmatute/Sources/GolfLeagueManager/backend/Business/Scores.csv"

if [ ! -f "$ORIGINAL_CSV" ]; then
    echo "❌ Original CSV file not found: $ORIGINAL_CSV"
    exit 1
fi

if [ ! -f "$CORRECTED_CSV" ]; then
    echo "❌ Corrected CSV file not found: $CORRECTED_CSV"
    exit 1
fi

echo "📁 Comparing score totals between original and corrected CSV files..."
echo ""

# Create a Python script to do the comparison
cat > /tmp/validate_scores.py << 'EOF'
#!/usr/bin/env python3
import csv
import sys

def parse_original_csv(filename):
    """Parse the original CSV and extract player scores by week/round"""
    scores = {}
    
    with open(filename, 'r') as file:
        reader = csv.DictReader(file)
        for row in reader:
            player = row['Player'].strip()
            week = int(float(row['Week_Number']))
            front_back = row['Front_or_Back'].strip()
            total_score = float(row['Total_Score']) if row['Total_Score'] else 0
            
            # Create key for this specific round
            key = f"{player}|{week}|{front_back}"
            scores[key] = {
                'player': player,
                'week': week,
                'front_back': front_back,
                'total': int(total_score),
                'holes': []
            }
            
            # Extract individual hole scores based on front/back
            if front_back == 'Front':
                # Front nine: extract holes 1-9
                for i in range(1, 10):
                    hole_key = f'Hole_{i}'
                    if hole_key in row and row[hole_key]:
                        scores[key]['holes'].append(int(float(row[hole_key])))
                    else:
                        scores[key]['holes'].append(0)
            elif front_back == 'Back':
                # Back nine: extract holes 10-18
                for i in range(10, 19):
                    hole_key = f'Hole_{i}'
                    if hole_key in row and row[hole_key]:
                        scores[key]['holes'].append(int(float(row[hole_key])))
                    else:
                        scores[key]['holes'].append(0)
    
    return scores

def parse_corrected_csv(filename):
    """Parse the corrected CSV and extract player scores"""
    scores = {}
    
    with open(filename, 'r') as file:
        lines = file.readlines()
        
    # Skip empty lines and header
    data_lines = []
    for line in lines:
        line = line.strip()
        if line and not line.startswith('Week,Front/Back'):
            data_lines.append(line)
    
    for line in data_lines:
        parts = line.split(',')
        if len(parts) >= 23:
            week = int(parts[0])
            front_back = parts[1].strip()
            first_name = parts[2].strip()
            last_name = parts[3].strip()
            player = f"{first_name} {last_name}"
            
            # Extract hole scores based on front/back
            holes = []
            if front_back == 'Front':
                # Front nine: extract holes 1-9 (columns 4-12)
                for i in range(4, 13):
                    if parts[i]:
                        holes.append(int(parts[i]))
                    else:
                        holes.append(0)
            elif front_back == 'Back':
                # Back nine: extract holes 10-18 (columns 13-21)
                for i in range(13, 22):
                    if parts[i]:
                        holes.append(int(parts[i]))
                    else:
                        holes.append(0)
            
            # Calculate total
            total = sum(holes)
            
            # Extract stated total from last column
            stated_total = int(parts[-1]) if parts[-1] else 0
            
            key = f"{player}|{week}|{front_back}"
            scores[key] = {
                'player': player,
                'week': week,
                'front_back': front_back,
                'total': total,
                'stated_total': stated_total,
                'holes': holes
            }
    
    return scores

def main():
    print("🔍 Parsing original CSV...")
    original_scores = parse_original_csv(sys.argv[1])
    
    print("🔍 Parsing corrected CSV...")
    corrected_scores = parse_corrected_csv(sys.argv[2])
    
    print(f"\n📊 Found {len(original_scores)} records in original CSV")
    print(f"📊 Found {len(corrected_scores)} records in corrected CSV")
    print()
    
    errors = []
    matches = 0
    
    # Check each original record
    for key, orig in original_scores.items():
        if key in corrected_scores:
            corr = corrected_scores[key]
            
            # Compare totals
            if orig['total'] != corr['total']:
                errors.append({
                    'key': key,
                    'type': 'total_mismatch',
                    'original': orig['total'],
                    'corrected': corr['total'],
                    'orig_holes': orig['holes'],
                    'corr_holes': corr['holes']
                })
            else:
                matches += 1
                
            # Check if corrected total matches calculated total
            if corr['total'] != corr['stated_total']:
                errors.append({
                    'key': key,
                    'type': 'calculation_error',
                    'calculated': corr['total'],
                    'stated': corr['stated_total'],
                    'holes': corr['holes']
                })
        else:
            errors.append({
                'key': key,
                'type': 'missing_record',
                'original': orig['total']
            })
    
    # Check for extra records in corrected CSV
    for key in corrected_scores:
        if key not in original_scores:
            errors.append({
                'key': key,
                'type': 'extra_record',
                'corrected': corrected_scores[key]['total']
            })
    
    print(f"✅ Matching records: {matches}")
    print(f"❌ Issues found: {len(errors)}")
    print()
    
    if errors:
        print("🚨 ISSUES DETECTED:")
        print("=" * 80)
        
        # Group errors by type
        total_mismatches = [e for e in errors if e['type'] == 'total_mismatch']
        calculation_errors = [e for e in errors if e['type'] == 'calculation_error']
        missing_records = [e for e in errors if e['type'] == 'missing_record']
        extra_records = [e for e in errors if e['type'] == 'extra_record']
        
        if total_mismatches:
            print(f"\n❌ TOTAL MISMATCHES ({len(total_mismatches)}):")
            for error in total_mismatches[:10]:  # Show first 10
                print(f"   {error['key']}: Original={error['original']}, Corrected={error['corrected']}")
                print(f"     Original holes: {error['orig_holes']}")
                print(f"     Corrected holes: {error['corr_holes']}")
                print()
            if len(total_mismatches) > 10:
                print(f"   ... and {len(total_mismatches) - 10} more")
        
        if calculation_errors:
            print(f"\n❌ CALCULATION ERRORS ({len(calculation_errors)}):")
            for error in calculation_errors[:10]:  # Show first 10
                print(f"   {error['key']}: Calculated={error['calculated']}, Stated={error['stated']}")
                print(f"     Holes: {error['holes']}")
                print()
            if len(calculation_errors) > 10:
                print(f"   ... and {len(calculation_errors) - 10} more")
        
        if missing_records:
            print(f"\n❌ MISSING RECORDS ({len(missing_records)}):")
            for error in missing_records[:10]:  # Show first 10
                print(f"   {error['key']}: Original total={error['original']}")
            if len(missing_records) > 10:
                print(f"   ... and {len(missing_records) - 10} more")
        
        if extra_records:
            print(f"\n❌ EXTRA RECORDS ({len(extra_records)}):")
            for error in extra_records[:10]:  # Show first 10
                print(f"   {error['key']}: Corrected total={error['corrected']}")
            if len(extra_records) > 10:
                print(f"   ... and {len(extra_records) - 10} more")
    else:
        print("🎉 ALL SCORES MATCH! Data validation successful.")
    
    return len(errors)

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python3 validate_scores.py <original_csv> <corrected_csv>")
        sys.exit(1)
    
    error_count = main()
    sys.exit(error_count if error_count < 255 else 255)
EOF

echo "🐍 Running Python validation script..."
python3 /tmp/validate_scores.py "$ORIGINAL_CSV" "$CORRECTED_CSV"

VALIDATION_RESULT=$?

echo ""
echo "================================================"
if [ $VALIDATION_RESULT -eq 0 ]; then
    echo "✅ VALIDATION PASSED: All scores match between original and corrected CSV"
else
    echo "❌ VALIDATION FAILED: Found issues that need to be fixed"
fi
echo "================================================"

# Clean up
rm -f /tmp/validate_scores.py

exit $VALIDATION_RESULT
