#!/bin/bash

# Script to delete all matchups from the database

API_BASE_URL="http://localhost:5154/api"

echo "Starting to delete all matchups..."

# Get all matchups
MATCHUPS=$(curl -s "${API_BASE_URL}/matchups")

# Extract IDs using jq and delete each one
echo "$MATCHUPS" | jq -r '.[].id' | while read -r id; do
    echo "Deleting matchup with ID: $id"
    RESPONSE=$(curl -s -w "\n%{http_code}" -X DELETE "${API_BASE_URL}/matchups/$id")
    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    
    if [ "$HTTP_CODE" -eq 200 ] || [ "$HTTP_CODE" -eq 204 ]; then
        echo "Successfully deleted matchup $id"
    else
        echo "Failed to delete matchup $id (HTTP $HTTP_CODE)"
        echo "Response: $(echo "$RESPONSE" | head -n -1)"
    fi
done

echo "Finished deleting all matchups"

# Verify no matchups remain
REMAINING_COUNT=$(curl -s "${API_BASE_URL}/matchups" | jq 'length')
echo "Remaining matchups: $REMAINING_COUNT"
