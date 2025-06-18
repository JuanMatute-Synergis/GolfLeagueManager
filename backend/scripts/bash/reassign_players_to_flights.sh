#!/bin/bash

# Golf League Players Flight Reassignment Script
# This script assigns players to their correct flights per user's specifications

API_BASE="http://localhost:5274/api"

# Function to get flight ID by name (only flights with seasonId)
get_flight_id() {
    local flight_name="$1"
    curl -s -X GET "$API_BASE/flights" | jq -r ".[] | select(.name == \"$flight_name\" and .seasonId != null) | .id"
}

# Function to get player ID by name
get_player_id() {
    local first_name="$1"
    local last_name="$2"
    curl -s -X GET "$API_BASE/players" | jq -r ".[] | select(.firstName == \"$first_name\" and .lastName == \"$last_name\") | .id"
}

# Function to assign player to flight
assign_player_to_flight() {
    local player_id="$1"
    local flight_id="$2"
    local handicap="$3"
    local player_name="$4"
    
    echo "Assigning $player_name to flight..."
    
    response=$(curl -s -X POST "$API_BASE/player-flight-assignments" \
        -H "Content-Type: application/json" \
        -d "{
            \"playerId\": \"$player_id\",
            \"flightId\": \"$flight_id\",
            \"handicapAtAssignment\": $handicap,
            \"isFlightLeader\": false
        }")
    
    if [ $? -eq 0 ]; then
        echo "✓ Successfully assigned $player_name"
    else
        echo "✗ Failed to assign $player_name"
        echo "Response: $response"
    fi
}

echo "Starting player flight reassignment process..."

# Get flight IDs
flight1_id=$(get_flight_id "1")
flight2_id=$(get_flight_id "2")
flight3_id=$(get_flight_id "3")
flight4_id=$(get_flight_id "4")

echo "Flight IDs:"
echo "Flight 1: $flight1_id"
echo "Flight 2: $flight2_id"
echo "Flight 3: $flight3_id"
echo "Flight 4: $flight4_id"

# Flight 1 Players - Handicap 44-45 (average 44.5)
echo ""
echo "=== Assigning Flight 1 Players ==="

players_flight1=(
    "George,Hutson"
    "Jeff,Dilcher"
    "Bill,Stein"
    "Alex,Peck"
    "Tim,Seyler"
    "Kevin,Kelhart"
    "Joe,Mahachanh"
    "John,Perry"
)

for player in "${players_flight1[@]}"; do
    IFS=',' read -r first_name last_name <<< "$player"
    player_id=$(get_player_id "$first_name" "$last_name")
    if [ ! -z "$player_id" ] && [ ! -z "$flight1_id" ]; then
        assign_player_to_flight "$player_id" "$flight1_id" "44.5" "$first_name $last_name"
    else
        echo "✗ Could not find player or flight: $first_name $last_name"
    fi
done

# Flight 2 Players - Handicap 41-44 (average 42.5)
echo ""
echo "=== Assigning Flight 2 Players ==="

players_flight2=(
    "Carl,Hardner"
    "Jay,Sullivan"
    "Stu,Silfies"
    "Steve,Bedek"
    "Curt,Saeger"
    "Lou,Gabrielle"
    "Frank,Frankenfield"
    "Kenny,Palladino"
)

for player in "${players_flight2[@]}"; do
    IFS=',' read -r first_name last_name <<< "$player"
    player_id=$(get_player_id "$first_name" "$last_name")
    if [ ! -z "$player_id" ] && [ ! -z "$flight2_id" ]; then
        assign_player_to_flight "$player_id" "$flight2_id" "42.5" "$first_name $last_name"
    else
        echo "✗ Could not find player or flight: $first_name $last_name"
    fi
done

# Flight 3 Players - Handicap 47-48 (average 47.5)
echo ""
echo "=== Assigning Flight 3 Players ==="

players_flight3=(
    "Matt,Speth"
    "Jim,Eck"
    "Kevin,Kelhart JR"
    "Steve,Hampton"
    "Bob,Gross"
    "Juan,Matute"
    "Matt,Donahue"
    "Danny,Washurn"
)

for player in "${players_flight3[@]}"; do
    IFS=',' read -r first_name last_name <<< "$player"
    player_id=$(get_player_id "$first_name" "$last_name")
    if [ ! -z "$player_id" ] && [ ! -z "$flight3_id" ]; then
        assign_player_to_flight "$player_id" "$flight3_id" "47.5" "$first_name $last_name"
    else
        echo "✗ Could not find player or flight: $first_name $last_name"
    fi
done

# Flight 4 Players - Handicap 50-58 (average 54)
echo ""
echo "=== Assigning Flight 4 Players ==="

players_flight4=(
    "Ray,Ballinger"
    "Rich,Hart"
    "Mike,Schaefer"
    "Steve,Kerns"
    "Steve,Filipovits"
    "Andrew,Kerns"
    "Tom,Haeusler"
    "Jax,Haeusler"
)

for player in "${players_flight4[@]}"; do
    IFS=',' read -r first_name last_name <<< "$player"
    player_id=$(get_player_id "$first_name" "$last_name")
    if [ ! -z "$player_id" ] && [ ! -z "$flight4_id" ]; then
        assign_player_to_flight "$player_id" "$flight4_id" "54" "$first_name $last_name"
    else
        echo "✗ Could not find player or flight: $first_name $last_name"
    fi
done

echo ""
echo "=== Flight reassignment completed! ==="
echo "Reassigned 32 players across 4 flights"
echo "Flight 1: 8 players (44-45 handicap)"
echo "Flight 2: 8 players (41-44 handicap)"
echo "Flight 3: 8 players (47-48 handicap)"
echo "Flight 4: 8 players (50-58 handicap)"

# Verify assignments
echo ""
echo "=== Verification ==="
total_assignments=$(curl -s -X GET "$API_BASE/player-flight-assignments" | jq length)
echo "Total player-flight assignments: $total_assignments"
