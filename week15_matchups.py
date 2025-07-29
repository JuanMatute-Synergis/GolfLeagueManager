#!/usr/bin/env python3
"""
Generate matchups for Week 15 and subsequent weeks based on the schedule
"""

# Player data from the file - organized by flight with positions 1-8 in each flight
flights = {
    "Flight 1": {
        1: "George Hutson",
        2: "Rich Baker", 
        3: "Jason Fink",
        4: "Bill Stein",
        5: "John Perry",
        6: "Alex Peck",
        7: "Jeff Dilcher",
        8: "Joe Mahachanh"
    },
    
    "Flight 2": {
        1: "Tim Seyler",
        2: "Kevin Kelhart",
        3: "Curt Saeger", 
        4: "Carl Hardner",
        5: "Kenny Palladino",
        6: "Frank Frankenfield",
        7: "Stu Silfies",
        8: "Ray Ballinger"
    },
    
    "Flight 3": {
        1: "Steve Bedek",
        2: "Lou Gabrielle",
        3: "Jim Eck",
        4: "Danny Washburn", 
        5: "Bob Gross",
        6: "Juan Matute",
        7: "Steve Hampton",
        8: "Kevin Kelhart JR"
    },
    
    "Flight 4": {
        1: "Matt Donahue",
        2: "Rich Hart",
        3: "Andrew Kerns",
        4: "Steve Kerns",
        5: "Ben Mahachanh",
        6: "Jax Haeusler",
        7: "Steve Filipovits", 
        8: "Tom Haeusler"
    }
}

# Schedule for session 3
schedule = [
    {"week": 15, "date": "7/30", "matchups": ["1 vs 2", "3 vs 8", "4 vs 7", "5 vs 6"]},
    {"week": 16, "date": "8/6", "matchups": ["1 vs 5", "2 vs 4", "3 vs 6", "7 vs 8"]},
    {"week": 17, "date": "8/13", "matchups": ["1 vs 3", "2 vs 7", "4 vs 5", "6 vs 8"]},
    {"week": 18, "date": "8/20", "matchups": ["1 vs 6", "2 vs 8", "3 vs 4", "5 vs 7"]},
    {"week": 19, "date": "8/27", "matchups": ["1 vs 8", "2 vs 5", "3 vs 7", "4 vs 6"]},
    {"week": 20, "date": "9/10", "matchups": ["1 vs 4", "2 vs 3", "5 vs 8", "6 vs 7"]},
    {"week": 21, "date": "9/17", "matchups": ["1 vs 7", "2 vs 6", "3 vs 5", "4 vs 8"]}
]

def create_flight_matchups(flight_name, week_data):
    """Create matchups for a specific flight based on the schedule"""
    matchups = []
    flight_players = flights[flight_name]
    
    for matchup_str in week_data["matchups"]:
        # Parse "1 vs 2" format
        parts = matchup_str.split(" vs ")
        player1_pos = int(parts[0])
        player2_pos = int(parts[1])
        
        player1_name = flight_players.get(player1_pos, f"Player {player1_pos}")
        player2_name = flight_players.get(player2_pos, f"Player {player2_pos}")
        
        matchups.append({
            "player1": {"position": player1_pos, "name": player1_name},
            "player2": {"position": player2_pos, "name": player2_name}
        })
    
    return matchups

def generate_all_matchups():
    """Generate matchups for all weeks and all flights"""
    all_matchups = []
    
    for week_data in schedule:
        week_matchups = {
            "week": week_data["week"],
            "date": week_data["date"],
            "flights": {}
        }
        
        # Generate matchups for each flight using positions 1-8
        for flight_name in flights.keys():
            week_matchups["flights"][flight_name] = create_flight_matchups(flight_name, week_data)
        
        all_matchups.append(week_matchups)
    
    return all_matchups

def print_matchups(matchups):
    """Print matchups in a readable format"""
    for week in matchups:
        print(f"\n{'='*70}")
        print(f"WEEK {week['week']} - {week['date']}")
        print(f"{'='*70}")
        
        for flight_name, flight_matchups in week["flights"].items():
            print(f"\n{flight_name}:")
            print("-" * 50)
            for i, matchup in enumerate(flight_matchups, 1):
                p1 = matchup["player1"]
                p2 = matchup["player2"]
                print(f"  Matchup {i}: {p1['name']} (pos {p1['position']}) vs {p2['name']} (pos {p2['position']})")

if __name__ == "__main__":
    print("GOLF LEAGUE MATCHUPS - SESSION 3")
    print("Starting from Week 15")
    
    matchups = generate_all_matchups()
    print_matchups(matchups)
    
    print(f"\n\nTotal weeks: {len(matchups)}")
    print("Total matchups per week: 16 (4 per flight x 4 flights)")
