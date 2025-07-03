#!/usr/bin/env python3
"""
Display all players' initial handicaps and average scores in a table format.

This script queries the API and shows the current state of all players'
initial handicaps and average scores after the Week 1 data import.

Usage:
    python scripts/database/show_player_table.py
"""

import requests
import json
from typing import List, Dict

# Configuration
API_BASE_URL = "http://localhost:5274/api"

def get_all_players() -> List[Dict]:
    """Fetch all players from the API."""
    try:
        response = requests.get(f"{API_BASE_URL}/players")
        if response.status_code == 200:
            return response.json()
        else:
            print(f"❌ Failed to fetch players: {response.status_code}")
            return []
    except Exception as e:
        print(f"❌ Error fetching players: {str(e)}")
        return []

def display_players_table(players: List[Dict]):
    """Display players in a formatted table."""
    if not players:
        print("❌ No players found")
        return
    
    # Sort players by initial handicap, then by name
    sorted_players = sorted(players, key=lambda p: (p.get('initialHandicap', 0), p.get('firstName', '')))
    
    # Calculate column widths
    name_width = max(len(f"{p.get('firstName', '')} {p.get('lastName', '')}") for p in sorted_players)
    name_width = max(name_width, 20)  # Minimum width
    
    # Print header
    print("\n🏌️ Golf League Manager - Player Initial Data")
    print("=" * 80)
    print(f"{'Player Name':<{name_width}} {'Phone':<15} {'Init Handicap':<13} {'Init Avg Score':<14} {'Curr Handicap':<13} {'Curr Avg Score':<14}")
    print("-" * 80)
    
    # Print each player
    for player in sorted_players:
        name = f"{player.get('firstName', '')} {player.get('lastName', '')}"
        phone = player.get('phone', 'N/A')[:14]  # Truncate if too long
        init_handicap = player.get('initialHandicap', 0)
        init_avg = player.get('initialAverageScore', 0.0)
        # Note: Current handicap is now calculated dynamically, not stored
        curr_avg = player.get('currentAverageScore', 0.0)
        
        print(f"{name:<{name_width}} {phone:<15} {init_handicap:<13.1f} {init_avg:<14.2f} {'Dynamic':<13} {curr_avg:<14.2f}")
    
    # Summary statistics
    print("-" * 80)
    total_players = len(sorted_players)
    avg_init_handicap = sum(p.get('initialHandicap', 0) for p in sorted_players) / total_players
    avg_init_score = sum(p.get('initialAverageScore', 0) for p in sorted_players) / total_players
    
    print(f"{'TOTALS/AVERAGES':<{name_width}} {'N/A':<15} {avg_init_handicap:<13.1f} {avg_init_score:<14.2f}")
    print(f"Total Players: {total_players}")

def show_handicap_distribution(players: List[Dict]):
    """Show distribution of handicaps."""
    handicaps = [p.get('initialHandicap', 0) for p in players]
    handicap_counts = {}
    
    for h in handicaps:
        handicap_counts[h] = handicap_counts.get(h, 0) + 1
    
    print("\n📊 Handicap Distribution:")
    print("-" * 30)
    for handicap in sorted(handicap_counts.keys()):
        count = handicap_counts[handicap]
        bar = "█" * count
        print(f"Handicap {handicap:2.0f}: {count:2d} players {bar}")

def main():
    """Main function to display player data."""
    print("🔍 Fetching all players from API...")
    
    players = get_all_players()
    
    if not players:
        print("❌ No players found or API error")
        return
    
    print(f"✅ Found {len(players)} players")
    
    # Display the main table
    display_players_table(players)
    
    # Show handicap distribution
    show_handicap_distribution(players)
    
    print(f"\n📝 Data fetched from: {API_BASE_URL}/players")

if __name__ == "__main__":
    main()
