#!/usr/bin/env python3
"""
Script to recreate the corrected CSV from the original CSV with proper data integrity.
This will split the Player names into First Name and Last Name columns while preserving
all the original score data including both front and back nine scores.
"""

import csv
import sys

def split_player_name(player_name):
    """Split player name into first and last name"""
    parts = player_name.strip().split()
    if len(parts) >= 2:
        first_name = parts[0]
        last_name = ' '.join(parts[1:])  # Handle names like "Kelhart JR"
    else:
        first_name = parts[0] if parts else ""
        last_name = ""
    return first_name, last_name

def main():
    original_csv = "/Users/juanmatute/Downloads/download.csv"
    corrected_csv = "/Users/juanmatute/Sources/GolfLeagueManager/backend/Business/Scores.csv"
    
    print("🔄 Recreating corrected CSV from original CSV...")
    print(f"📖 Reading from: {original_csv}")
    print(f"💾 Writing to: {corrected_csv}")
    
    with open(original_csv, 'r') as infile, open(corrected_csv, 'w', newline='') as outfile:
        reader = csv.DictReader(infile)
        
        # Write header for corrected CSV
        outfile.write("Week,Front/Back,First Name,Last Name,Hole 1,Hole 2,Hole 3,Hole 4,Hole 5,Hole 6,Hole 7,Hole 8,Hole 9,Hole 10,Hole 11,Hole 12,Hole 13,Hole 14,Hole 15,Hole 16,Hole 17,Hole 18,Hole Total\n")
        
        processed_count = 0
        
        for row in reader:
            player = row['Player'].strip()
            week = int(float(row['Week_Number'])) if row['Week_Number'] else 0
            front_back = row['Front_or_Back'].strip()
            total_score = int(float(row['Total_Score'])) if row['Total_Score'] else 0
            
            # Split player name
            first_name, last_name = split_player_name(player)
            
            # Start building the row
            corrected_row = [str(week), front_back, first_name, last_name]
            
            # Add hole scores based on front/back
            if front_back == 'Front':
                # Add front nine scores (holes 1-9)
                for i in range(1, 10):
                    hole_key = f'Hole_{i}'
                    score = int(float(row[hole_key])) if row[hole_key] else 0
                    corrected_row.append(str(score))
                
                # Add empty back nine (holes 10-18)
                for i in range(10, 19):
                    corrected_row.append("")
            
            elif front_back == 'Back':
                # Add empty front nine (holes 1-9)  
                for i in range(1, 10):
                    corrected_row.append("")
                
                # Add back nine scores (holes 10-18)
                for i in range(10, 19):
                    hole_key = f'Hole_{i}'
                    score = int(float(row[hole_key])) if row[hole_key] else 0
                    corrected_row.append(str(score))
            
            # Add total score
            corrected_row.append(str(total_score))
            
            # Write the row
            outfile.write(','.join(corrected_row) + '\n')
            processed_count += 1
            
            if processed_count % 20 == 0:
                print(f"✅ Processed {processed_count} records...")
    
    print(f"🎉 Successfully recreated corrected CSV with {processed_count} records!")
    print("📊 Verifying data integrity...")
    
    # Quick verification
    with open(corrected_csv, 'r') as file:
        lines = file.readlines()
        data_lines = [line for line in lines if line.strip() and not line.startswith('Week,Front/Back')]
        front_count = sum(1 for line in data_lines if ',Front,' in line)
        back_count = sum(1 for line in data_lines if ',Back,' in line)
        
        print(f"✅ Front nine records: {front_count}")
        print(f"✅ Back nine records: {back_count}")
        print(f"✅ Total records: {len(data_lines)}")

if __name__ == "__main__":
    main()
