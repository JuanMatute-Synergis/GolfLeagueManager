#!/usr/bin/env python3
"""
Script to update player emails from a provided email list.
Matches players by name and updates their emails via the API.
"""

import json
import re
import requests
from typing import Dict, List, Optional

# API configuration
API_BASE_URL = "http://localhost:5274"
PLAYERS_ENDPOINT = f"{API_BASE_URL}/api/players"

# Email list from the file
EMAIL_LIST = """
Andy Evans <AndyEvansHPE@gmail.com>,
Andy Folk <andy.folk@gmail.com>,
Ashish Purani <ashish.purani@gmail.com>,
Bill Almond <Cbalmond3@yahoo.com>,
Bill Hawk <kalthehawk65@aol.com>,
Billy Hawk <hawkfam@ptd.net>,
Bob Meyers <rmeyers57@gmail.com>,
Brian Hamilton <Bham3860@yahoo.com>,
Brian McNeal <brianmcneal22@gmail.com>,
Bryce Meyers <BMeyers@northamptoncounty.org>,
Charlie Wernette <snbrd190@hotmail.com>,
Daniel Bogle <dboggle@msn.com>,
Dave Deschler <desch1228@gmail.com>,
Dave Hall II <davesdeliandgelato@rcn.com>,
Dave Hall OG <dhall1514@gmail.com>,
Dave Jones <dpjonesiii73@gmail.com>,
Dave Pinter <samaxbear@yahoo.com>,
Dean Paulus <dapaulus2421@gmail.com>,
Dennis Thomas <d_thomas181@aol.com>,
Doran Hamann <djhamn@yahoo.com>,
Drew <andrewfreed79@yahoo.com>,
fischerheck <fischerheck@aol.com>,
Gary Kantor <Gary.Kantor@yahoo.com>,
Gary Matika <gbkmatika@aol.com>,
George Zumas <gz9751@yahoo.com>,
Gino Mercadante <gmerc1103@gmail.com>,
Greg Grim <fordtruck88@msn.com>,
Greg Smith <swordfish0361@yahoo.com>,
Jack Savitske <jsavitske@ptd.net>,
Jeff Silfies <Jeff.Silfies@rcn.com>,
Joe Christiano <jchristiano19@gmail.com>,
Joe Seidenberger <nautica318@aol.com>,
Jon Bard <jonbard@earthlink.net>,
Jordan Zaffrin <Jmanskater1010@aol.com>,
Juan Matute <juan.matute@gmail.com>,
Justin Kinter <jckinter@gmail.com>,
Keith Lehman <kslehman59@gmail.com>,
Kenny <kdpaulus93@gmail.com>,
Kim Gehris <Kimgehris0721@gmail.com>,
Marcus <mgichiengo2456@gmail.com>,
Mark Wimmer <eags56@yahoo.com>,
Mike Becker <becker.michael@rcn.com>,
Mike Darrell <zumapa@rcn.com>,
Mike Marchese <michaelgmarchese@outlook.com>,
Mike Skubik <mscooby65@hotmail.com>,
Owen Hawk <Ohawk442@gmail.com>,
Paul Albert <Palbert5@ptd.net>,
Paul Colahan <pcolahan1@gmail.com>,
Paul Kametz <pkametz@gmail.com>,
Peter Mandes <mandesfam@msn.com>,
Peter Mandes <yankidog67@gmail.com>,
Phil Goetz <agoetz@ptd.net>,
Phil Hawk <hawkmusic73@gmail.com>,
Rich Carnathan <richy6488@aim.com>,
Rich Kozicki <rk93054@aol.com>,
Ryan Crisman <ryan@southmooregolf.com>,
Scott James <scottjames425@gmail.com>,
Steve Kuchera <sjkuchera@gmail.com>,
Steve Rice <steamedrice@hotmail.com>,
Tom Ament <tament66@msn.com>,
Tom Donahue <td33@hcsc33.biz>,
Trevor Colahan <trevor.colahan@gmail.com>
"""

def parse_email_list(email_text: str) -> Dict[str, str]:
    """
    Parse the email list into a dictionary of {name: email}.
    Handles formats like "Name <email@domain.com>," and variations.
    """
    email_dict = {}
    
    # Pattern to match "Name <email@domain.com>" format
    pattern = r'([^<,]+?)\s*<([^>]+)>'
    
    matches = re.findall(pattern, email_text)
    
    for name, email in matches:
        name = name.strip()
        email = email.strip()
        
        # Normalize name for matching
        normalized_name = normalize_name_for_matching(name)
        email_dict[normalized_name] = email
        
        print(f"Parsed: '{name}' -> '{email}'")
    
    return email_dict

def normalize_name_for_matching(name: str) -> str:
    """
    Normalize names for better matching.
    Handles variations like "Dave Jones" vs "Dave Jones III", etc.
    """
    # Remove common suffixes and variations
    name = re.sub(r'\s+(II|III|IV|OG|Jr\.?|Sr\.?)$', '', name, flags=re.IGNORECASE)
    
    # Convert to lowercase for case-insensitive matching
    return name.lower().strip()

def find_player_matches(players: List[dict], email_dict: Dict[str, str]) -> List[tuple]:
    """
    Find matches between players and the email list.
    Returns list of (player, new_email) tuples.
    """
    matches = []
    
    for player in players:
        player_name = f"{player['firstName']} {player['lastName']}"
        normalized_player_name = normalize_name_for_matching(player_name)
        
        # Try exact match first
        if normalized_player_name in email_dict:
            new_email = email_dict[normalized_player_name]
            matches.append((player, new_email))
            print(f"✓ Exact match: {player_name} -> {new_email}")
            continue
        
        # Try partial matches (first name + last name)
        first_name = normalize_name_for_matching(player['firstName'])
        last_name = normalize_name_for_matching(player['lastName'])
        
        for email_name, email in email_dict.items():
            if first_name in email_name and last_name in email_name:
                matches.append((player, email))
                print(f"✓ Partial match: {player_name} -> {email}")
                break
        else:
            print(f"✗ No match found for: {player_name}")
    
    return matches

def update_player_email(player: dict, new_email: str) -> bool:
    """
    Update a player's email via the API.
    """
    player_id = player['id']
    
    # Create updated player object
    updated_player = {
        'id': player_id,
        'firstName': player['firstName'],
        'lastName': player['lastName'],
        'email': new_email,
        'phone': player['phone']
    }
    
    try:
        response = requests.put(f"{PLAYERS_ENDPOINT}/{player_id}", json=updated_player)
        response.raise_for_status()
        return True
    except requests.exceptions.RequestException as e:
        print(f"Error updating {player['firstName']} {player['lastName']}: {e}")
        return False

def main():
    print("🏌️ Starting player email update process...\n")
    
    # Step 1: Get current players
    print("📥 Fetching current players from API...")
    try:
        response = requests.get(PLAYERS_ENDPOINT)
        response.raise_for_status()
        players = response.json()
        print(f"Found {len(players)} players in the system.\n")
    except requests.exceptions.RequestException as e:
        print(f"❌ Error fetching players: {e}")
        return
    
    # Step 2: Parse email list
    print("📋 Parsing email list...")
    email_dict = parse_email_list(EMAIL_LIST)
    print(f"Parsed {len(email_dict)} email entries.\n")
    
    # Step 3: Find matches
    print("🔍 Finding matches between players and email list...")
    matches = find_player_matches(players, email_dict)
    print(f"\nFound {len(matches)} matches to update.\n")
    
    if not matches:
        print("No matches found. Exiting.")
        return
    
    # Step 4: Confirm before updating
    print("📋 Summary of updates to be made:")
    for player, new_email in matches:
        old_email = player['email']
        print(f"  {player['firstName']} {player['lastName']}: {old_email} -> {new_email}")
    
    confirm = input(f"\n❓ Do you want to proceed with updating {len(matches)} players? (y/N): ")
    if confirm.lower() != 'y':
        print("❌ Update cancelled.")
        return
    
    # Step 5: Update players
    print(f"\n🔄 Updating {len(matches)} players...")
    success_count = 0
    
    for player, new_email in matches:
        player_name = f"{player['firstName']} {player['lastName']}"
        if update_player_email(player, new_email):
            print(f"✅ Updated {player_name}")
            success_count += 1
        else:
            print(f"❌ Failed to update {player_name}")
    
    print(f"\n🎉 Update complete! Successfully updated {success_count}/{len(matches)} players.")

if __name__ == "__main__":
    main()
