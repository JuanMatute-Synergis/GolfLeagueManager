#!/bin/bash

# Clean up duplicate Week 24 entries
API_BASE="http://localhost:5505/api"

echo "Cleaning up duplicate Week 24 entries..."

# Get all Week 24 IDs
week_24_ids=($(curl -s -X GET "$API_BASE/weeks" | jq -r '.[] | select(.weekNumber == 24) | .id'))

echo "Found ${#week_24_ids[@]} Week 24 entries"

# Keep the first one, delete the rest
for ((i=1; i<${#week_24_ids[@]}; i++)); do
    week_id=${week_24_ids[$i]}
    echo "Deleting duplicate Week 24 with ID: $week_id"
    
    response=$(curl -s -X DELETE "$API_BASE/weeks/$week_id")
    if [ -z "$response" ]; then
        echo "✓ Successfully deleted Week 24 (ID: $week_id)"
    else
        echo "✗ Failed to delete Week 24 (ID: $week_id)"
        echo "Response: $response"
    fi
done

echo ""
echo "Remaining Week 24:"
curl -s -X GET "$API_BASE/weeks" | jq '.[] | select(.weekNumber == 24)'
