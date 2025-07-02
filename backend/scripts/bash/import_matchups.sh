#!/bin/bash

# Golf League Matchups Import Script
# This script creates matchups from the CSV file

API_BASE="http://localhost:5505/api"
CSV_FILE="/Users/juanmatute/Downloads/golf_league_named_matchups.csv"

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
    
    # Convert date format from M/D to YYYY-MM-DD
    local month=$(echo "$date" | cut -d'/' -f1)
    local day=$(echo "$date" | cut -d'/' -f2)
    
    # Pad with zeros
    month=$(printf "%02d" "$month")
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
    
    echo "Creating matchup: $player1_name vs $player2_name (Date: $date, Week: $week_id)"
    
    response=$(curl -s -X POST "$API_BASE/matchups" \
        -H "Content-Type: application/json" \
        -d "{
            \"player1Id\": \"$player1_id\",
            \"player2Id\": \"$player2_id\",
            \"weekId\": \"$week_id\",
            \"scheduledDate\": \"2025-$(printf '%02d' $(echo $date | cut -d'/' -f1))-$(printf '%02d' $(echo $date | cut -d'/' -f2))T00:00:00Z\"
        }")
    
    if echo "$response" | jq -e '.id' > /dev/null 2>&1; then
        echo "✓ Successfully created matchup"
    else
        echo "✗ Failed to create matchup"
        echo "Response: $response"
    fi
}

echo "Starting matchup import process..."
echo "Reading CSV file: $CSV_FILE"

# Skip header line and process each matchup
tail -n +2 "$CSV_FILE" | while IFS=',' read -r week date matchup; do
    # Parse player names from matchup (format: "Player1 Name vs Player2 Name")
    player1_full=$(echo "$matchup" | awk -F' vs ' '{print $1}')
    player2_full=$(echo "$matchup" | awk -F' vs ' '{print $2}')
    
    # Split full names into first and last names
    player1_first=$(echo "$player1_full" | awk '{print $1}')
    player1_last=$(echo "$player1_full" | awk '{$1=""; print $0}' | sed 's/^ *//')
    
    player2_first=$(echo "$player2_full" | awk '{print $1}')
    player2_last=$(echo "$player2_full" | awk '{$1=""; print $0}' | sed 's/^ *//')
    
    # Get player IDs
    player1_id=$(get_player_id "$player1_first" "$player1_last")
    player2_id=$(get_player_id "$player2_first" "$player2_last")
    
    # Get week ID based on date
    week_id=$(get_week_id_by_date "$date")
    
    # Create matchup if all IDs are found
    if [ ! -z "$player1_id" ] && [ ! -z "$player2_id" ] && [ ! -z "$week_id" ]; then
        create_matchup "$player1_id" "$player2_id" "$week_id" "$player1_full" "$player2_full" "$date"
    else
        echo "✗ Could not create matchup: $matchup (Date: $date)"
        echo "  Player1 ID: $player1_id ($player1_full)"
        echo "  Player2 ID: $player2_id ($player2_full)"
        echo "  Week ID: $week_id"
    fi
    
    # Small delay to avoid overwhelming the API
    sleep 0.1
done

echo ""
echo "=== Matchup import completed! ==="

# Verify total matchups created
total_matchups=$(curl -s -X GET "$API_BASE/matchups" | jq length)
echo "Total matchups in system: $total_matchups"

# Show matchups by week
echo ""
echo "=== Matchups by Week ==="
curl -s -X GET "$API_BASE/matchups" | jq -r 'group_by(.weekId) | map({weekId: .[0].weekId, count: length}) | .[] | "Week \(.weekId): \(.count) matchups"'
