#!/bin/bash

# Golf League Players Import Script
# This script adds players from the CSV file and assigns them to flights

API_BASE="http://localhost:5505/api"

# Function to add a player
add_player() {
    local first_name="$1"
    local last_name="$2"
    local phone="$3"
    local handicap="$4"
    
    # Create email from name (simple format)
    local email=$(echo "${first_name}.${last_name}@golfleague.com" | tr '[:upper:]' '[:lower:]' | tr ' ' '.')
    
    echo "Adding player: $first_name $last_name"
    
    curl -s -X POST "$API_BASE/players" \
        -H "Content-Type: application/json" \
        -d "{
            \"firstName\": \"$first_name\",
            \"lastName\": \"$last_name\",
            \"email\": \"$email\",
            \"phone\": \"$phone\"
        }" | jq -r '.id'
}

# Function to get flight ID by name
get_flight_id() {
    local flight_name="$1"
    curl -s -X GET "$API_BASE/flights" | jq -r ".[] | select(.name == \"$flight_name\" and .seasonId != null) | .id"
}

# Function to assign player to flight
assign_player_to_flight() {
    local player_id="$1"
    local flight_id="$2"
    local handicap="$3"
    
    echo "Assigning player to flight..."
    
    curl -s -X POST "$API_BASE/player-flight-assignments" \
        -H "Content-Type: application/json" \
        -d "{
            \"playerId\": \"$player_id\",
            \"flightId\": \"$flight_id\",
            \"handicapAtAssignment\": $handicap,
            \"isFlightLeader\": false
        }"
}

# Function to extract handicap number from range (e.g., "41–44" -> 42.5)
extract_handicap() {
    local handicap_range="$1"
    # Extract numbers and calculate average
    local min=$(echo "$handicap_range" | cut -d'–' -f1)
    local max=$(echo "$handicap_range" | cut -d'–' -f2)
    echo "scale=1; ($min + $max) / 2" | bc
}

echo "Starting player import process..."

# Flight 1 Players (Handicap 41-44)
echo "=== Adding Flight 1 Players ==="

player_id=$(add_player "Frank" "Frankenfield" "484-375-9035")
flight_id=$(get_flight_id "1")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "42.5"
fi

player_id=$(add_player "Kenny" "Palladino" "(610) 657-9977")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "42.5"
fi

player_id=$(add_player "Lou" "Gabrielle" "484-951-0821")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "42.5"
fi

player_id=$(add_player "Steve" "Bedek" "484-274-1325")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "42.5"
fi

player_id=$(add_player "Curt" "Saeger" "610-392-8228")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "42.5"
fi

player_id=$(add_player "Jay" "Sullivan" "610-476-4418")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "42.5"
fi

player_id=$(add_player "Stu" "Silfies" "484-547-9553")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "42.5"
fi

player_id=$(add_player "Carl" "Hardner" "484-664-9193")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "42.5"
fi

# Flight 2 Players (Handicap 44-45)
echo "=== Adding Flight 2 Players ==="

player_id=$(add_player "John" "Perry" "610-530-9092")
flight_id=$(get_flight_id "2")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "44.5"
fi

player_id=$(add_player "Kevin" "Kelhart" "610-597-3315")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "44.5"
fi

player_id=$(add_player "Joe" "Mahachanh" "267-382-8223")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "44.5"
fi

player_id=$(add_player "Alex" "Peck" "908-442-2036")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "44.5"
fi

player_id=$(add_player "Tim" "Seyler" "(484) 343-4123")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "44.5"
fi

player_id=$(add_player "Jeff" "Dilcher" "215-804-7664")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "44.5"
fi

player_id=$(add_player "Bill" "Stein" "484-695-9692")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "44.5"
fi

player_id=$(add_player "George" "Hutson" "610-428-4032")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "44.5"
fi

# Flight 3 Players (Handicap 47-48)
echo "=== Adding Flight 3 Players ==="

player_id=$(add_player "Matt" "Donahue" "570-851-7148")
flight_id=$(get_flight_id "3")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "47.5"
fi

player_id=$(add_player "Danny" "Washurn" "484-357-8773")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "47.5"
fi

player_id=$(add_player "Bob" "Gross" "484-866-5195")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "47.5"
fi

player_id=$(add_player "Juan" "Matute" "610-417-7659")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "47.5"
fi

player_id=$(add_player "Kevin" "Kelhart JR" "484-821-8295")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "47.5"
fi

player_id=$(add_player "Steve" "Hampton" "610-442-2483")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "47.5"
fi

player_id=$(add_player "Matt" "Speth" "484-788-3417")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "47.5"
fi

player_id=$(add_player "Jim" "Eck" "610-858-6264")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "47.5"
fi

# Flight 4 Players (Handicap 50-58)
echo "=== Adding Flight 4 Players ==="

player_id=$(add_player "Tom" "Haeusler" "610-217-7491")
flight_id=$(get_flight_id "4")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "54"
fi

player_id=$(add_player "Jax" "Haeusler" "484-635-9062")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "54"
fi

player_id=$(add_player "Steve" "Filipovits" "610-823-2206")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "54"
fi

player_id=$(add_player "Andrew" "Kerns" "484-401-1000")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "54"
fi

player_id=$(add_player "Mike" "Schaefer" "484-553-0734")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "54"
fi

player_id=$(add_player "Steve" "Kerns" "484-256-4012")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "54"
fi

player_id=$(add_player "Ray" "Ballinger" "484-554-9044")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "54"
fi

player_id=$(add_player "Rich" "Hart" "610-533-7423")
if [ ! -z "$flight_id" ] && [ ! -z "$player_id" ]; then
    assign_player_to_flight "$player_id" "$flight_id" "54"
fi

echo "=== Player import completed! ==="
echo "Added 32 players across 4 flights"
echo "Flight 1: 8 players (Handicap 41-44)"
echo "Flight 2: 8 players (Handicap 44-45)" 
echo "Flight 3: 8 players (Handicap 47-48)"
echo "Flight 4: 8 players (Handicap 50-58)"
