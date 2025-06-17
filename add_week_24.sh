#!/bin/bash

# Add Week 24 to cover September 17-23, 2025
# This will allow the remaining Week 21 matchups to be imported

API_BASE="http://localhost:5274/api"
SEASON_ID="42eb1d90-0ac0-46c2-b64f-02acd6e92760"

echo "Adding Week 24 for September 17-23, 2025..."

response=$(curl -s -X POST "$API_BASE/weeks" \
  -H "Content-Type: application/json" \
  -d '{
    "weekNumber": 24,
    "date": "2025-09-17T00:00:00.000Z",
    "name": "Week 24",
    "isActive": true,
    "seasonId": "'$SEASON_ID'"
  }')

if echo "$response" | jq -e '.id' > /dev/null 2>&1; then
    echo "✓ Successfully created Week 24"
    echo "Week ID: $(echo "$response" | jq -r '.id')"
else
    echo "✗ Failed to create Week 24"
    echo "Response: $response"
    exit 1
fi

echo ""
echo "Now you can re-run the import script to create the remaining 16 matchups for Week 21."
