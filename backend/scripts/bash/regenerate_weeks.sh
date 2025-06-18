#!/bin/bash

# Regenerate weeks for the current season
# This will delete all existing weeks and regenerate them using the correct logic

API_BASE="http://localhost:5274/api"

echo "=== Regenerating weeks for the season ==="

# Get the current season ID
season_id=$(curl -s "$API_BASE/seasons" | jq -r '.[0].id')
echo "Season ID: $season_id"

# Delete all existing weeks for this season
echo "Deleting existing weeks..."
week_ids=$(curl -s "$API_BASE/weeks" | jq -r ".[] | select(.seasonId == \"$season_id\") | .id")

for week_id in $week_ids; do
    echo "Deleting week: $week_id"
    curl -s -X DELETE "$API_BASE/weeks/$week_id"
done

# Wait a moment for deletions to complete
sleep 2

# Regenerate weeks using the corrected logic
echo "Regenerating weeks..."
curl -s -X POST "$API_BASE/seasons/$season_id/generate-weeks"

echo "=== Week regeneration completed ==="

# Verify the new weeks
echo "New weeks:"
curl -s "$API_BASE/weeks" | jq '.[] | {weekNumber, date}' | head -10
