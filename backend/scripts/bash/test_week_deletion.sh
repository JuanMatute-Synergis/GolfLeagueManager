#!/bin/bash

# Test script to verify week deletion and renumbering
# This script will test deleting a week and check if renumbering works correctly

API_BASE="http://localhost:5000/api"

echo "=== Testing Week Deletion and Renumbering ==="

# Get the first season
echo "Getting seasons..."
SEASON_RESPONSE=$(curl -s "$API_BASE/seasons")
SEASON_ID=$(echo "$SEASON_RESPONSE" | jq -r '.[0].id')
echo "Using season ID: $SEASON_ID"

if [ "$SEASON_ID" = "null" ] || [ -z "$SEASON_ID" ]; then
    echo "No seasons found. Please create a season first."
    exit 1
fi

# Get weeks for this season before deletion
echo -e "\n=== Weeks BEFORE deletion ==="
WEEKS_BEFORE=$(curl -s "$API_BASE/weeks/season/$SEASON_ID")
echo "$WEEKS_BEFORE" | jq -r '.[] | "Week \(.weekNumber): \(.name) (\(.date | split("T")[0]))"' | sort -n

# Count weeks
WEEK_COUNT_BEFORE=$(echo "$WEEKS_BEFORE" | jq length)
echo "Total weeks before: $WEEK_COUNT_BEFORE"

# Find a week to delete (let's try to delete week 8 if it exists)
WEEK_TO_DELETE_ID=$(echo "$WEEKS_BEFORE" | jq -r '.[] | select(.weekNumber == 8) | .id')

if [ "$WEEK_TO_DELETE_ID" = "null" ] || [ -z "$WEEK_TO_DELETE_ID" ]; then
    # If no week 8, delete the 5th week (middle of the season)
    WEEK_TO_DELETE_ID=$(echo "$WEEKS_BEFORE" | jq -r '.[4].id // empty')
    WEEK_TO_DELETE_NUM=$(echo "$WEEKS_BEFORE" | jq -r '.[4].weekNumber // empty')
else
    WEEK_TO_DELETE_NUM=8
fi

if [ -z "$WEEK_TO_DELETE_ID" ]; then
    echo "No suitable week found to delete. Need at least 5 weeks."
    exit 1
fi

echo -e "\n=== Deleting Week $WEEK_TO_DELETE_NUM (ID: $WEEK_TO_DELETE_ID) ==="

# Delete the week
DELETE_RESPONSE=$(curl -s -X DELETE "$API_BASE/weeks/$WEEK_TO_DELETE_ID")
echo "Delete response: $DELETE_RESPONSE"

# Wait a moment for the operation to complete
sleep 1

# Get weeks after deletion
echo -e "\n=== Weeks AFTER deletion ==="
WEEKS_AFTER=$(curl -s "$API_BASE/weeks/season/$SEASON_ID")
echo "$WEEKS_AFTER" | jq -r '.[] | "Week \(.weekNumber): \(.name) (\(.date | split("T")[0]))"' | sort -n

# Count weeks after
WEEK_COUNT_AFTER=$(echo "$WEEKS_AFTER" | jq length)
echo "Total weeks after: $WEEK_COUNT_AFTER"

# Check if renumbering worked correctly
echo -e "\n=== Renumbering Analysis ==="
echo "Expected: Week numbers should be sequential (1, 2, 3, ..., $WEEK_COUNT_AFTER)"
echo "Expected: Week names should match week numbers (Week 1, Week 2, etc.)"

# Check for gaps in week numbers
WEEK_NUMBERS=$(echo "$WEEKS_AFTER" | jq -r '.[].weekNumber' | sort -n)
EXPECTED_SEQUENCE=$(seq 1 $WEEK_COUNT_AFTER)

echo "Actual week numbers: $(echo "$WEEK_NUMBERS" | tr '\n' ' ')"
echo "Expected sequence:   $(echo "$EXPECTED_SEQUENCE" | tr '\n' ' ')"

# Check week names
echo -e "\nWeek Names Analysis:"
echo "$WEEKS_AFTER" | jq -r '.[] | "Week \(.weekNumber): Name=\"\(.name)\""' | sort -n

# Check if week numbers match
NUMBERS_MATCH=true
if [ "$(echo "$WEEK_NUMBERS" | tr '\n' ' ')" != "$(echo "$EXPECTED_SEQUENCE" | tr '\n' ' ')" ]; then
    NUMBERS_MATCH=false
fi

# Check if week names match the pattern "Week X"
NAMES_MATCH=true
while IFS= read -r week_data; do
    WEEK_NUM=$(echo "$week_data" | jq -r '.weekNumber')
    WEEK_NAME=$(echo "$week_data" | jq -r '.name')
    EXPECTED_NAME="Week $WEEK_NUM"
    if [ "$WEEK_NAME" != "$EXPECTED_NAME" ]; then
        echo "❌ Week $WEEK_NUM has name '$WEEK_NAME', expected '$EXPECTED_NAME'"
        NAMES_MATCH=false
    fi
done <<< "$(echo "$WEEKS_AFTER" | jq -c '.[]')"

if [ "$NUMBERS_MATCH" = true ] && [ "$NAMES_MATCH" = true ]; then
    echo "✅ SUCCESS: Week renumbering and relabeling worked correctly!"
else
    echo "❌ FAILURE: Week renumbering/relabeling did not work correctly!"
    if [ "$NUMBERS_MATCH" = false ]; then
        echo "  - Week numbers have gaps or incorrect sequence"
    fi
    if [ "$NAMES_MATCH" = false ]; then
        echo "  - Week names do not match expected pattern"
    fi
fi

echo -e "\n=== Test Complete ==="
