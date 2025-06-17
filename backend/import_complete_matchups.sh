#!/bin/bash

# Golf League Matchups Import Script - Complete Season
# This script creates all matchups from the provided RTF document

API_BASE="http://localhost:5274/api"

# Function to get player ID by name
get_player_id() {
    local first_name="$1"
    local last_name="$2"
    curl -s -X GET "$API_BASE/players" | jq -r ".[] | select(.firstName == \"$first_name\" and .lastName == \"$last_name\") | .id"
}

# Function to get week ID by date
get_week_id_by_date() {
    local date="$1"
    local year="2025"
    
    # Convert date format from "Month Day" to YYYY-MM-DD
    local month_name=$(echo "$date" | awk '{print $1}')
    local day=$(echo "$date" | awk '{print $2}')
    
    # Convert month name to number
    case $month_name in
        "April") month="04" ;;
        "May") month="05" ;;
        "June") month="06" ;;
        "July") month="07" ;;
        "August") month="08" ;;
        "September") month="09" ;;
        *) month="01" ;;
    esac
    
    # Pad day with zero if needed
    day=$(printf "%02d" "$day")
    
    local full_date="$year-$month-$day"
    
    # Find week that contains this date (now using single date field)
    curl -s -X GET "$API_BASE/weeks" | jq -r ".[] | select(.date | startswith(\"${full_date}\")) | .id"
}

# Function to create a matchup
create_matchup() {
    local player1_id="$1"
    local player2_id="$2"
    local week_id="$3"
    local player1_name="$4"
    local player2_name="$5"
    local date="$6"
    local flight="$7"
    
    echo "Creating $flight matchup: $player1_name vs $player2_name (Date: $date)"
    
    # Convert date to proper format for API
    local month_name=$(echo "$date" | awk '{print $1}')
    local day=$(echo "$date" | awk '{print $2}')
    
    case $month_name in
        "April") month="04" ;;
        "May") month="05" ;;
        "June") month="06" ;;
        "July") month="07" ;;
        "August") month="08" ;;
        "September") month="09" ;;
        *) month="01" ;;
    esac
    
    day=$(printf "%02d" "$day")
    
    response=$(curl -s -X POST "$API_BASE/matchups" \
        -H "Content-Type: application/json" \
        -d "{
            \"playerAId\": \"$player1_id\",
            \"playerBId\": \"$player2_id\",
            \"weekId\": \"$week_id\",
            \"scheduledDate\": \"2025-$month-${day}T00:00:00Z\"
        }")
    
    if echo "$response" | jq -e '.id' > /dev/null 2>&1; then
        echo "✓ Successfully created matchup"
    else
        echo "✗ Failed to create matchup"
        echo "Response: $response"
    fi
}

# Function to create matchups for a specific date and flight
create_flight_matchups() {
    local date="$1"
    local flight="$2"
    shift 2
    local matchups=("$@")
    
    echo ""
    echo "=== $date - $flight ==="
    
    local week_id=$(get_week_id_by_date "$date")
    
    for matchup in "${matchups[@]}"; do
        # Parse matchup (format: "Player1 Name vs Player2 Name")
        local player1_full=$(echo "$matchup" | awk -F' vs ' '{print $1}')
        local player2_full=$(echo "$matchup" | awk -F' vs ' '{print $2}')
        
        # Split full names into first and last names
        local player1_first=$(echo "$player1_full" | awk '{print $1}')
        local player1_last=$(echo "$player1_full" | awk '{$1=""; print $0}' | sed 's/^ *//')
        
        local player2_first=$(echo "$player2_full" | awk '{print $1}')
        local player2_last=$(echo "$player2_full" | awk '{$1=""; print $0}' | sed 's/^ *//')
        
        # Get player IDs
        local player1_id=$(get_player_id "$player1_first" "$player1_last")
        local player2_id=$(get_player_id "$player2_first" "$player2_last")
        
        # Create matchup if all IDs are found
        if [ ! -z "$player1_id" ] && [ ! -z "$player2_id" ] && [ ! -z "$week_id" ]; then
            create_matchup "$player1_id" "$player2_id" "$week_id" "$player1_full" "$player2_full" "$date" "$flight"
        else
            echo "✗ Could not create matchup: $matchup"
            echo "  Player1 ID: $player1_id ($player1_full)"
            echo "  Player2 ID: $player2_id ($player2_full)"
            echo "  Week ID: $week_id"
        fi
        
        # Small delay to avoid overwhelming the API
        sleep 0.1
    done
}

echo "Starting comprehensive matchup import process..."

# April 9
create_flight_matchups "April 9" "Flight 1" \
    "George Hutson vs Jeff Dilcher" \
    "Bill Stein vs John Perry" \
    "Alex Peck vs Joe Mahachanh" \
    "Tim Seyler vs Kevin Kelhart"

create_flight_matchups "April 9" "Flight 2" \
    "Carl Hardner vs Jay Sullivan" \
    "Stu Silfies vs Kenny Palladino" \
    "Steve Bedek vs Frank Frankenfield" \
    "Curt Saeger vs Lou Gabrielle"

create_flight_matchups "April 9" "Flight 3" \
    "Matt Speth vs Jim Eck" \
    "Kevin Kelhart JR vs Danny Washurn" \
    "Steve Hampton vs Matt Donahue" \
    "Bob Gross vs Juan Matute"

create_flight_matchups "April 9" "Flight 4" \
    "Ray Ballinger vs Rich Hart" \
    "Mike Schaefer vs Jax Haeusler" \
    "Steve Kerns vs Tom Haeusler" \
    "Steve Filipovits vs Andrew Kerns"

# April 16
create_flight_matchups "April 16" "Flight 1" \
    "Jeff Dilcher vs Alex Peck" \
    "George Hutson vs Tim Seyler" \
    "Bill Stein vs Kevin Kelhart" \
    "Joe Mahachanh vs John Perry"

create_flight_matchups "April 16" "Flight 2" \
    "Jay Sullivan vs Steve Bedek" \
    "Carl Hardner vs Curt Saeger" \
    "Stu Silfies vs Lou Gabrielle" \
    "Frank Frankenfield vs Kenny Palladino"

create_flight_matchups "April 16" "Flight 3" \
    "Matt Speth vs Bob Gross" \
    "Jim Eck vs Steve Hampton" \
    "Kevin Kelhart JR vs Juan Matute" \
    "Matt Donahue vs Danny Washurn"

create_flight_matchups "April 16" "Flight 4" \
    "Ray Ballinger vs Steve Filipovits" \
    "Rich Hart vs Steve Kerns" \
    "Mike Schaefer vs Andrew Kerns" \
    "Tom Haeusler vs Jax Haeusler"

# April 23
create_flight_matchups "April 23" "Flight 1" \
    "George Hutson vs Bill Stein" \
    "Jeff Dilcher vs Joe Mahachanh" \
    "Alex Peck vs Tim Seyler" \
    "Kevin Kelhart vs John Perry"

create_flight_matchups "April 23" "Flight 2" \
    "Carl Hardner vs Stu Silfies" \
    "Jay Sullivan vs Frank Frankenfield" \
    "Steve Bedek vs Curt Saeger" \
    "Lou Gabrielle vs Kenny Palladino"

create_flight_matchups "April 23" "Flight 3" \
    "Matt Speth vs Kevin Kelhart JR" \
    "Jim Eck vs Matt Donahue" \
    "Steve Hampton vs Bob Gross" \
    "Juan Matute vs Danny Washurn"

create_flight_matchups "April 23" "Flight 4" \
    "Ray Ballinger vs Mike Schaefer" \
    "Rich Hart vs Tom Haeusler" \
    "Steve Kerns vs Steve Filipovits" \
    "Andrew Kerns vs Jax Haeusler"

# April 30
create_flight_matchups "April 30" "Flight 1" \
    "Jeff Dilcher vs John Perry" \
    "George Hutson vs Kevin Kelhart" \
    "Bill Stein vs Alex Peck" \
    "Tim Seyler vs Joe Mahachanh"

create_flight_matchups "April 30" "Flight 2" \
    "Jay Sullivan vs Kenny Palladino" \
    "Carl Hardner vs Lou Gabrielle" \
    "Stu Silfies vs Steve Bedek" \
    "Curt Saeger vs Frank Frankenfield"

create_flight_matchups "April 30" "Flight 3" \
    "Matt Speth vs Juan Matute" \
    "Jim Eck vs Danny Washurn" \
    "Kevin Kelhart JR vs Steve Hampton" \
    "Bob Gross vs Matt Donahue"

create_flight_matchups "April 30" "Flight 4" \
    "Ray Ballinger vs Andrew Kerns" \
    "Rich Hart vs Jax Haeusler" \
    "Mike Schaefer vs Steve Kerns" \
    "Steve Filipovits vs Tom Haeusler"

# May 7
create_flight_matchups "May 7" "Flight 1" \
    "Jeff Dilcher vs Tim Seyler" \
    "George Hutson vs John Perry" \
    "Bill Stein vs Joe Mahachanh" \
    "Alex Peck vs Kevin Kelhart"

create_flight_matchups "May 7" "Flight 2" \
    "Jay Sullivan vs Curt Saeger" \
    "Carl Hardner vs Kenny Palladino" \
    "Stu Silfies vs Frank Frankenfield" \
    "Steve Bedek vs Lou Gabrielle"

create_flight_matchups "May 7" "Flight 3" \
    "Matt Speth vs Danny Washurn" \
    "Jim Eck vs Bob Gross" \
    "Kevin Kelhart JR vs Matt Donahue" \
    "Steve Hampton vs Juan Matute"

create_flight_matchups "May 7" "Flight 4" \
    "Ray Ballinger vs Jax Haeusler" \
    "Rich Hart vs Steve Filipovits" \
    "Mike Schaefer vs Tom Haeusler" \
    "Steve Kerns vs Andrew Kerns"

# May 14
create_flight_matchups "May 14" "Flight 1" \
    "George Hutson vs Alex Peck" \
    "Jeff Dilcher vs Bill Stein" \
    "Tim Seyler vs John Perry" \
    "Kevin Kelhart vs Joe Mahachanh"

create_flight_matchups "May 14" "Flight 2" \
    "Carl Hardner vs Steve Bedek" \
    "Jay Sullivan vs Stu Silfies" \
    "Curt Saeger vs Kenny Palladino" \
    "Lou Gabrielle vs Frank Frankenfield"

create_flight_matchups "May 14" "Flight 3" \
    "Matt Speth vs Steve Hampton" \
    "Jim Eck vs Kevin Kelhart JR" \
    "Bob Gross vs Danny Washurn" \
    "Juan Matute vs Matt Donahue"

create_flight_matchups "May 14" "Flight 4" \
    "Ray Ballinger vs Steve Kerns" \
    "Rich Hart vs Mike Schaefer" \
    "Steve Filipovits vs Jax Haeusler" \
    "Andrew Kerns vs Tom Haeusler"

# May 21
create_flight_matchups "May 21" "Flight 1" \
    "George Hutson vs Joe Mahachanh" \
    "Jeff Dilcher vs Kevin Kelhart" \
    "Bill Stein vs Tim Seyler" \
    "Alex Peck vs John Perry"

create_flight_matchups "May 21" "Flight 2" \
    "Carl Hardner vs Frank Frankenfield" \
    "Jay Sullivan vs Lou Gabrielle" \
    "Stu Silfies vs Curt Saeger" \
    "Steve Bedek vs Kenny Palladino"

create_flight_matchups "May 21" "Flight 3" \
    "Matt Speth vs Matt Donahue" \
    "Jim Eck vs Juan Matute" \
    "Kevin Kelhart JR vs Bob Gross" \
    "Steve Hampton vs Danny Washurn"

create_flight_matchups "May 21" "Flight 4" \
    "Ray Ballinger vs Tom Haeusler" \
    "Rich Hart vs Andrew Kerns" \
    "Mike Schaefer vs Steve Filipovits" \
    "Steve Kerns vs Jax Haeusler"

echo ""
echo "=== Matchup import completed for first 7 weeks! ==="

# Continue with remaining weeks...

# June 4 (Week 8)
create_flight_matchups "June 4" "Flight 1" \
    "George Hutson vs Jeff Dilcher" \
    "Bill Stein vs John Perry" \
    "Alex Peck vs Joe Mahachanh" \
    "Tim Seyler vs Kevin Kelhart"

create_flight_matchups "June 4" "Flight 2" \
    "Carl Hardner vs Jay Sullivan" \
    "Stu Silfies vs Kenny Palladino" \
    "Steve Bedek vs Frank Frankenfield" \
    "Curt Saeger vs Lou Gabrielle"

create_flight_matchups "June 4" "Flight 3" \
    "Matt Speth vs Jim Eck" \
    "Kevin Kelhart JR vs Danny Washurn" \
    "Steve Hampton vs Matt Donahue" \
    "Bob Gross vs Juan Matute"

create_flight_matchups "June 4" "Flight 4" \
    "Ray Ballinger vs Rich Hart" \
    "Mike Schaefer vs Jax Haeusler" \
    "Steve Kerns vs Tom Haeusler" \
    "Steve Filipovits vs Andrew Kerns"

# June 11 (Week 9)
create_flight_matchups "June 11" "Flight 1" \
    "Jeff Dilcher vs Alex Peck" \
    "George Hutson vs Tim Seyler" \
    "Bill Stein vs Kevin Kelhart" \
    "Joe Mahachanh vs John Perry"

create_flight_matchups "June 11" "Flight 2" \
    "Jay Sullivan vs Steve Bedek" \
    "Carl Hardner vs Curt Saeger" \
    "Stu Silfies vs Lou Gabrielle" \
    "Frank Frankenfield vs Kenny Palladino"

create_flight_matchups "June 11" "Flight 3" \
    "Matt Speth vs Bob Gross" \
    "Jim Eck vs Steve Hampton" \
    "Kevin Kelhart JR vs Juan Matute" \
    "Matt Donahue vs Danny Washurn"

create_flight_matchups "June 11" "Flight 4" \
    "Ray Ballinger vs Steve Filipovits" \
    "Rich Hart vs Steve Kerns" \
    "Mike Schaefer vs Andrew Kerns" \
    "Tom Haeusler vs Jax Haeusler"

# June 18 (Week 10)
create_flight_matchups "June 18" "Flight 1" \
    "George Hutson vs Bill Stein" \
    "Jeff Dilcher vs Joe Mahachanh" \
    "Alex Peck vs Tim Seyler" \
    "Kevin Kelhart vs John Perry"

create_flight_matchups "June 18" "Flight 2" \
    "Carl Hardner vs Stu Silfies" \
    "Jay Sullivan vs Frank Frankenfield" \
    "Steve Bedek vs Curt Saeger" \
    "Lou Gabrielle vs Kenny Palladino"

create_flight_matchups "June 18" "Flight 3" \
    "Matt Speth vs Kevin Kelhart JR" \
    "Jim Eck vs Matt Donahue" \
    "Steve Hampton vs Bob Gross" \
    "Juan Matute vs Danny Washurn"

create_flight_matchups "June 18" "Flight 4" \
    "Ray Ballinger vs Mike Schaefer" \
    "Rich Hart vs Tom Haeusler" \
    "Steve Kerns vs Steve Filipovits" \
    "Andrew Kerns vs Jax Haeusler"

# June 25 (Week 11)
create_flight_matchups "June 25" "Flight 1" \
    "Jeff Dilcher vs John Perry" \
    "George Hutson vs Kevin Kelhart" \
    "Bill Stein vs Alex Peck" \
    "Tim Seyler vs Joe Mahachanh"

create_flight_matchups "June 25" "Flight 2" \
    "Jay Sullivan vs Kenny Palladino" \
    "Carl Hardner vs Lou Gabrielle" \
    "Stu Silfies vs Steve Bedek" \
    "Curt Saeger vs Frank Frankenfield"

create_flight_matchups "June 25" "Flight 3" \
    "Matt Speth vs Juan Matute" \
    "Jim Eck vs Danny Washurn" \
    "Kevin Kelhart JR vs Steve Hampton" \
    "Bob Gross vs Matt Donahue"

create_flight_matchups "June 25" "Flight 4" \
    "Ray Ballinger vs Andrew Kerns" \
    "Rich Hart vs Jax Haeusler" \
    "Mike Schaefer vs Steve Kerns" \
    "Steve Filipovits vs Tom Haeusler"

# July 9 (Week 12)
create_flight_matchups "July 9" "Flight 1" \
    "Jeff Dilcher vs Tim Seyler" \
    "George Hutson vs John Perry" \
    "Bill Stein vs Joe Mahachanh" \
    "Alex Peck vs Kevin Kelhart"

create_flight_matchups "July 9" "Flight 2" \
    "Jay Sullivan vs Curt Saeger" \
    "Carl Hardner vs Kenny Palladino" \
    "Stu Silfies vs Frank Frankenfield" \
    "Steve Bedek vs Lou Gabrielle"

create_flight_matchups "July 9" "Flight 3" \
    "Matt Speth vs Danny Washurn" \
    "Jim Eck vs Bob Gross" \
    "Kevin Kelhart JR vs Matt Donahue" \
    "Steve Hampton vs Juan Matute"

create_flight_matchups "July 9" "Flight 4" \
    "Ray Ballinger vs Jax Haeusler" \
    "Rich Hart vs Steve Filipovits" \
    "Mike Schaefer vs Tom Haeusler" \
    "Steve Kerns vs Andrew Kerns"

# July 16 (Week 13)
create_flight_matchups "July 16" "Flight 1" \
    "George Hutson vs Alex Peck" \
    "Jeff Dilcher vs Bill Stein" \
    "Tim Seyler vs John Perry" \
    "Kevin Kelhart vs Joe Mahachanh"

create_flight_matchups "July 16" "Flight 2" \
    "Carl Hardner vs Steve Bedek" \
    "Jay Sullivan vs Stu Silfies" \
    "Curt Saeger vs Kenny Palladino" \
    "Lou Gabrielle vs Frank Frankenfield"

create_flight_matchups "July 16" "Flight 3" \
    "Matt Speth vs Steve Hampton" \
    "Jim Eck vs Kevin Kelhart JR" \
    "Bob Gross vs Danny Washurn" \
    "Juan Matute vs Matt Donahue"

create_flight_matchups "July 16" "Flight 4" \
    "Ray Ballinger vs Steve Kerns" \
    "Rich Hart vs Mike Schaefer" \
    "Steve Filipovits vs Jax Haeusler" \
    "Andrew Kerns vs Tom Haeusler"

# July 23 (Week 14)
create_flight_matchups "July 23" "Flight 1" \
    "George Hutson vs Joe Mahachanh" \
    "Jeff Dilcher vs Kevin Kelhart" \
    "Bill Stein vs Tim Seyler" \
    "Alex Peck vs John Perry"

create_flight_matchups "July 23" "Flight 2" \
    "Carl Hardner vs Frank Frankenfield" \
    "Jay Sullivan vs Lou Gabrielle" \
    "Stu Silfies vs Curt Saeger" \
    "Steve Bedek vs Kenny Palladino"

create_flight_matchups "July 23" "Flight 3" \
    "Matt Speth vs Matt Donahue" \
    "Jim Eck vs Juan Matute" \
    "Kevin Kelhart JR vs Bob Gross" \
    "Steve Hampton vs Danny Washurn"

create_flight_matchups "July 23" "Flight 4" \
    "Ray Ballinger vs Tom Haeusler" \
    "Rich Hart vs Andrew Kerns" \
    "Mike Schaefer vs Steve Filipovits" \
    "Steve Kerns vs Jax Haeusler"

# July 30 (Week 15)
create_flight_matchups "July 30" "Flight 1" \
    "George Hutson vs Jeff Dilcher" \
    "Bill Stein vs John Perry" \
    "Alex Peck vs Joe Mahachanh" \
    "Tim Seyler vs Kevin Kelhart"

create_flight_matchups "July 30" "Flight 2" \
    "Carl Hardner vs Jay Sullivan" \
    "Stu Silfies vs Kenny Palladino" \
    "Steve Bedek vs Frank Frankenfield" \
    "Curt Saeger vs Lou Gabrielle"

create_flight_matchups "July 30" "Flight 3" \
    "Matt Speth vs Jim Eck" \
    "Kevin Kelhart JR vs Danny Washurn" \
    "Steve Hampton vs Matt Donahue" \
    "Bob Gross vs Juan Matute"

create_flight_matchups "July 30" "Flight 4" \
    "Ray Ballinger vs Rich Hart" \
    "Mike Schaefer vs Jax Haeusler" \
    "Steve Kerns vs Tom Haeusler" \
    "Steve Filipovits vs Andrew Kerns"

# August 6 (Week 16)
create_flight_matchups "August 6" "Flight 1" \
    "Jeff Dilcher vs Alex Peck" \
    "George Hutson vs Tim Seyler" \
    "Bill Stein vs Kevin Kelhart" \
    "Joe Mahachanh vs John Perry"

create_flight_matchups "August 6" "Flight 2" \
    "Jay Sullivan vs Steve Bedek" \
    "Carl Hardner vs Curt Saeger" \
    "Stu Silfies vs Lou Gabrielle" \
    "Frank Frankenfield vs Kenny Palladino"

create_flight_matchups "August 6" "Flight 3" \
    "Matt Speth vs Bob Gross" \
    "Jim Eck vs Steve Hampton" \
    "Kevin Kelhart JR vs Juan Matute" \
    "Matt Donahue vs Danny Washurn"

create_flight_matchups "August 6" "Flight 4" \
    "Ray Ballinger vs Steve Filipovits" \
    "Rich Hart vs Steve Kerns" \
    "Mike Schaefer vs Andrew Kerns" \
    "Tom Haeusler vs Jax Haeusler"

# August 13 (Week 17)
create_flight_matchups "August 13" "Flight 1" \
    "George Hutson vs Bill Stein" \
    "Jeff Dilcher vs Joe Mahachanh" \
    "Alex Peck vs Tim Seyler" \
    "Kevin Kelhart vs John Perry"

create_flight_matchups "August 13" "Flight 2" \
    "Carl Hardner vs Stu Silfies" \
    "Jay Sullivan vs Frank Frankenfield" \
    "Steve Bedek vs Curt Saeger" \
    "Lou Gabrielle vs Kenny Palladino"

create_flight_matchups "August 13" "Flight 3" \
    "Matt Speth vs Kevin Kelhart JR" \
    "Jim Eck vs Matt Donahue" \
    "Steve Hampton vs Bob Gross" \
    "Juan Matute vs Danny Washurn"

create_flight_matchups "August 13" "Flight 4" \
    "Ray Ballinger vs Mike Schaefer" \
    "Rich Hart vs Tom Haeusler" \
    "Steve Kerns vs Steve Filipovits" \
    "Andrew Kerns vs Jax Haeusler"

# August 20 (Week 18)
create_flight_matchups "August 20" "Flight 1" \
    "Jeff Dilcher vs John Perry" \
    "George Hutson vs Kevin Kelhart" \
    "Bill Stein vs Alex Peck" \
    "Tim Seyler vs Joe Mahachanh"

create_flight_matchups "August 20" "Flight 2" \
    "Jay Sullivan vs Kenny Palladino" \
    "Carl Hardner vs Lou Gabrielle" \
    "Stu Silfies vs Steve Bedek" \
    "Curt Saeger vs Frank Frankenfield"

create_flight_matchups "August 20" "Flight 3" \
    "Matt Speth vs Juan Matute" \
    "Jim Eck vs Danny Washurn" \
    "Kevin Kelhart JR vs Steve Hampton" \
    "Bob Gross vs Matt Donahue"

create_flight_matchups "August 20" "Flight 4" \
    "Ray Ballinger vs Andrew Kerns" \
    "Rich Hart vs Jax Haeusler" \
    "Mike Schaefer vs Steve Kerns" \
    "Steve Filipovits vs Tom Haeusler"

# August 27 (Week 19)
create_flight_matchups "August 27" "Flight 1" \
    "Jeff Dilcher vs Tim Seyler" \
    "George Hutson vs John Perry" \
    "Bill Stein vs Joe Mahachanh" \
    "Alex Peck vs Kevin Kelhart"

create_flight_matchups "August 27" "Flight 2" \
    "Jay Sullivan vs Curt Saeger" \
    "Carl Hardner vs Kenny Palladino" \
    "Stu Silfies vs Frank Frankenfield" \
    "Steve Bedek vs Lou Gabrielle"

create_flight_matchups "August 27" "Flight 3" \
    "Matt Speth vs Danny Washurn" \
    "Jim Eck vs Bob Gross" \
    "Kevin Kelhart JR vs Matt Donahue" \
    "Steve Hampton vs Juan Matute"

create_flight_matchups "August 27" "Flight 4" \
    "Ray Ballinger vs Jax Haeusler" \
    "Rich Hart vs Steve Filipovits" \
    "Mike Schaefer vs Tom Haeusler" \
    "Steve Kerns vs Andrew Kerns"

# September 10 (Week 20)
create_flight_matchups "September 10" "Flight 1" \
    "George Hutson vs Alex Peck" \
    "Jeff Dilcher vs Bill Stein" \
    "Tim Seyler vs John Perry" \
    "Kevin Kelhart vs Joe Mahachanh"

create_flight_matchups "September 10" "Flight 2" \
    "Carl Hardner vs Steve Bedek" \
    "Jay Sullivan vs Stu Silfies" \
    "Curt Saeger vs Kenny Palladino" \
    "Lou Gabrielle vs Frank Frankenfield"

create_flight_matchups "September 10" "Flight 3" \
    "Matt Speth vs Steve Hampton" \
    "Jim Eck vs Kevin Kelhart JR" \
    "Bob Gross vs Danny Washurn" \
    "Juan Matute vs Matt Donahue"

create_flight_matchups "September 10" "Flight 4" \
    "Ray Ballinger vs Steve Kerns" \
    "Rich Hart vs Mike Schaefer" \
    "Steve Filipovits vs Jax Haeusler" \
    "Andrew Kerns vs Tom Haeusler"

# September 17 (Week 21)
create_flight_matchups "September 17" "Flight 1" \
    "George Hutson vs Joe Mahachanh" \
    "Jeff Dilcher vs Kevin Kelhart" \
    "Bill Stein vs Tim Seyler" \
    "Alex Peck vs John Perry"

create_flight_matchups "September 17" "Flight 2" \
    "Carl Hardner vs Frank Frankenfield" \
    "Jay Sullivan vs Lou Gabrielle" \
    "Stu Silfies vs Curt Saeger" \
    "Steve Bedek vs Kenny Palladino"

create_flight_matchups "September 17" "Flight 3" \
    "Matt Speth vs Matt Donahue" \
    "Jim Eck vs Juan Matute" \
    "Kevin Kelhart JR vs Bob Gross" \
    "Steve Hampton vs Danny Washurn"

create_flight_matchups "September 17" "Flight 4" \
    "Ray Ballinger vs Tom Haeusler" \
    "Rich Hart vs Andrew Kerns" \
    "Mike Schaefer vs Steve Filipovits" \
    "Steve Kerns vs Jax Haeusler"

echo ""
echo "=== COMPLETE MATCHUP IMPORT FINISHED! ==="
echo "Successfully imported matchups for all 21 weeks of the season"

# Verify total matchups created so far
total_matchups=$(curl -s -X GET "$API_BASE/matchups" | jq length)
echo "Total matchups in system: $total_matchups"
