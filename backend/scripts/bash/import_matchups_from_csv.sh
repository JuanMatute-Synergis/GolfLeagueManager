#!/bin/bash

# Golf League Matchups Import Script - Import from CSV
# This script imports matchups from the corrected CSV file

API_BASE="http://localhost:5505/api"
CSV_FILE="../data/schedule_with_names_corrected.csv"

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
    
    # Find week that contains this date (exact date match for Wednesday)
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
        return 0
    else
        echo "✗ Failed to create matchup"
        echo "Response: $response"
        return 1
    fi
}

echo "Starting matchup import from CSV: $CSV_FILE"

# Check if CSV file exists
if [ ! -f "$CSV_FILE" ]; then
    echo "Error: CSV file not found: $CSV_FILE"
    exit 1
fi

# Initialize counters
total_count=0
success_count=0
error_count=0

# Create temporary file to process CSV line by line
temp_file=$(mktemp)
tail -n +2 "$CSV_FILE" > "$temp_file"

# Process each matchup
while IFS=',' read -r week date flight matchup; do
    total_count=$((total_count + 1))
    
    echo ""
    echo "=== Processing Week $week - $date - $flight ==="
    echo "Matchup: $matchup"
    
    # Parse matchup (format: "Player1 Name vs Player2 Name")
    player1_full=$(echo "$matchup" | awk -F' vs ' '{print $1}')
    player2_full=$(echo "$matchup" | awk -F' vs ' '{print $2}')
    
    # Split full names into first and last names
    player1_first=$(echo "$player1_full" | awk '{print $1}')
    player1_last=$(echo "$player1_full" | awk '{$1=""; print $0}' | sed 's/^ *//')
    
    player2_first=$(echo "$player2_full" | awk '{print $1}')
    player2_last=$(echo "$player2_full" | awk '{$1=""; print $0}' | sed 's/^ *//')
    
    # Get week ID
    week_id=$(get_week_id_by_date "$date")
    
    # Get player IDs
    player1_id=$(get_player_id "$player1_first" "$player1_last")
    player2_id=$(get_player_id "$player2_first" "$player2_last")
    
    # Create matchup if all IDs are found
    if [ ! -z "$player1_id" ] && [ ! -z "$player2_id" ] && [ ! -z "$week_id" ]; then
        if create_matchup "$player1_id" "$player2_id" "$week_id" "$player1_full" "$player2_full" "$date" "$flight"; then
            success_count=$((success_count + 1))
        else
            error_count=$((error_count + 1))
        fi
    else
        echo "✗ Could not create matchup: $matchup"
        echo "  Player1 ID: $player1_id ($player1_full)"
        echo "  Player2 ID: $player2_id ($player2_full)"
        echo "  Week ID: $week_id"
        error_count=$((error_count + 1))
    fi
    
    # Small delay to avoid overwhelming the API
    sleep 0.1
done < "$temp_file"

# Clean up temporary file
rm "$temp_file"

echo ""
echo "=== MATCHUP IMPORT COMPLETED ==="
echo "Total processed: $total_count"
echo "Successfully created: $success_count"
echo "Errors: $error_count"

# Verify total matchups created
total_matchups=$(curl -s -X GET "$API_BASE/matchups" | jq length)
echo "Total matchups in system: $total_matchups"
