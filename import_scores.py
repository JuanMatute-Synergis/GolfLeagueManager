#!/usr/bin/env python3
"""
Golf League Score Import Script
This script reads the CSV file and imports scores via the API
"""

import csv
import json
import requests
import sys
from pathlib import Path

def read_csv_file(csv_path):
    """Read the CSV file and return the content as a string"""
    try:
        with open(csv_path, 'r', encoding='utf-8') as file:
            return file.read()
    except Exception as e:
        print(f"Error reading CSV file: {e}")
        return None

def import_scores(csv_content):
    """Send the CSV content to the import API"""
    url = "http://localhost:5274/api/import/scores"
    
    payload = {
        "csvContent": csv_content
    }
    
    headers = {
        "Content-Type": "application/json"
    }
    
    try:
        print("Sending import request to API...")
        response = requests.post(url, json=payload, headers=headers)
        
        if response.status_code == 200:
            result = response.json()
            print(f"✅ Import successful!")
            print(f"Message: {result.get('message', '')}")
            
            if result.get('errors'):
                print(f"\n⚠️  Errors encountered:")
                for error in result['errors']:
                    print(f"  - {error}")
        else:
            print(f"❌ Import failed with status {response.status_code}")
            print(f"Response: {response.text}")
            
    except requests.exceptions.ConnectionError:
        print("❌ Could not connect to the API. Make sure the backend server is running on localhost:5274")
    except Exception as e:
        print(f"❌ Error during import: {e}")

def main():
    # Path to the CSV file
    csv_path = "/Users/juanmatute/Downloads/download.csv"
    
    if not Path(csv_path).exists():
        print(f"❌ CSV file not found: {csv_path}")
        sys.exit(1)
    
    print(f"📁 Reading CSV file: {csv_path}")
    csv_content = read_csv_file(csv_path)
    
    if csv_content is None:
        sys.exit(1)
    
    print(f"📊 CSV file loaded ({len(csv_content)} characters)")
    
    # Import the scores
    import_scores(csv_content)

if __name__ == "__main__":
    main()
