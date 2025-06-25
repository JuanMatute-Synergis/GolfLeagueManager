#!/bin/bash

# Update Session 2 Initial Averages for Golf League Players
# Using the correct PlayerSessionAverage API endpoint

# Server URL
API_URL="http://localhost:5274/api"

# Season ID for 2025 (retrieved earlier)
SEASON_ID="a57df491-9860-4c01-a883-ab68e838adb7"

# Session 2 starts at week 8
SESSION_START_WEEK=8

# Function to update player session average
update_player_session_average() {
    local player_id=$1
    local average=$2
    local player_name=$3
    
    echo "Updating $player_name (ID: $player_id) with Session 2 average: $average"
    
    # Make API call to create/update the player's Session 2 initial average
    response=$(curl -s -X POST \
        -H "Content-Type: application/json" \
        -d "{\"initialAverage\": $average}" \
        "$API_URL/PlayerSessionAverage/player/$player_id/season/$SEASON_ID/session/$SESSION_START_WEEK")
    
    if [ $? -eq 0 ]; then
        echo "✓ Successfully updated $player_name"
    else
        echo "✗ Failed to update $player_name"
        echo "Response: $response"
    fi
    echo "---"
}

# Player averages data from the golf league sheet
# Format: "Player Name" average

echo "Starting Session 2 Initial Average updates..."
echo "Season ID: $SEASON_ID"
echo "Session Start Week: $SESSION_START_WEEK"
echo "============================================="

# Update each player's Session 2 initial average
update_player_session_average "238b9732-4a0f-4922-8d5c-26e33fd8ac95" 43.73 "Juan Matute"
update_player_session_average "7d6db2a5-480b-4e68-a0f7-bd5d24db8999" 39.67 "Stu Silfies"
update_player_session_average "459e64fe-a08e-4f1b-8546-91b9cb10ed95" 41.93 "Steve Bedek"
update_player_session_average "52369d98-5f1b-4c9d-9859-3d4a2a183439" 46.40 "Frank Frankenfield"
update_player_session_average "354f62e4-a538-4eaa-b7e4-5f9bd60e777f" 40.53 "Ray Ballinger"
update_player_session_average "ed6799bc-76d6-43fd-9301-f4fa9effaee6" 44.60 "Rich Hart"
update_player_session_average "2c8b669d-8b6c-4951-880f-a3d607415f8c" 47.00 "George Hutson"
update_player_session_average "d971efb9-8e8c-4e91-87e9-f79c74391455" 46.27 "Jeff Dilcher"
update_player_session_average "c80aef4d-db47-493e-b1ae-9d09a734a094" 43.87 "Alex Peck"
update_player_session_average "9548bbdf-f6ea-447d-988b-fe1e21bd2080" 49.07 "Joe Mahachanh"
update_player_session_average "4ef1e93d-be97-42f6-9721-3c1ddd256c60" 48.40 "Tim Seyler"
update_player_session_average "1cbaa69a-f72e-4ccd-9e6b-2ea1a501f5ee" 44.07 "Kevin Kelhart"
update_player_session_average "28fca147-da6f-4fbb-a4bd-67e51f17c2a2" 43.67 "Jay Sullivan"
update_player_session_average "1e083524-cc5e-436b-a911-63c4607f5a33" 47.40 "Mike Schaefer"
update_player_session_average "b1046572-8971-425b-9dbd-cdfdc80bbdd7" 42.73 "Steve Kerns"
update_player_session_average "ac3946ff-48eb-4479-ad63-92136e7c2f7f" 43.93 "Curt Saeger"
update_player_session_average "4d2ab22d-daee-4cbf-98ca-ca5f8034b3fc" 41.20 "Jim Eck"
update_player_session_average "155adf40-c73d-4c46-b2e2-f6b2471563dd" 44.27 "Steve Hampton"
update_player_session_average "74d4dd50-3a5a-4e14-8e06-a37704cddd71" 41.40 "Matt Donahue"
update_player_session_average "3a58bd78-1074-4c5c-b43e-605a8efc3a35" 44.73 "Bob Gross"
update_player_session_average "c43faa83-23d6-4d09-9ffb-9548361480bc" 51.13 "Kevin Kelhart JR"
update_player_session_average "9deacc93-f9ed-49e6-ab68-c89b056af0fe" 48.53 "Danny Washurn"
update_player_session_average "dd5a8bd3-0bf1-436e-9484-74b40d6e83fc" 46.07 "Bill Stein"
update_player_session_average "8c24e1d3-50bf-49b0-956f-3843ab1d9bf7" 44.80 "John Perry"
update_player_session_average "731fbc13-0f87-497a-9674-1bceaa67b395" 47.13 "Carl Hardner"
update_player_session_average "878f2662-b5bb-4474-aaa5-d2b4b179e070" 42.33 "Lou Gabrielle"
update_player_session_average "9e7a3048-2254-46ae-a16a-a90cc8dbd962" 39.80 "Matt Speth"
update_player_session_average "a51ea62e-4a3f-4741-b43c-56e3feae7e70" 43.07 "Kenny Palladino"
update_player_session_average "c7de2e43-847d-4b73-bb40-cf268fa7c301" 44.13 "Jax Haeusler"
update_player_session_average "67a0c954-df29-45ec-a183-08ff7cb3678b" 44.87 "Tom Haeusler"
update_player_session_average "9d4ab9eb-6c6f-4fdd-be6b-2396202eb645" 42.27 "Steve Filipovits"
update_player_session_average "a16dd095-e46d-4f47-8767-50b02226dca4" 41.73 "Andrew Kerns"

echo "============================================="
echo "Session 2 Initial Average updates completed!"
echo ""
echo "To verify the updates, you can check a few players by calling:"
echo "curl -s \"$API_URL/PlayerSessionAverage/player/{playerId}/season/$SEASON_ID/session/$SESSION_START_WEEK\" | jq"
