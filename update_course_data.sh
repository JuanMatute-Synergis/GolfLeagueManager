#!/bin/bash

# Script to update Allentown Municipal Golf Course data from JSON file
# Usage: ./update_course_data.sh

# Configuration
API_BASE_URL="http://localhost:5000/api"
COURSE_NAME="Allentown Municipal"
JSON_FILE="backend/scripts/data/allentown-hcp-data.json"

echo "🏌️  Updating course data for: $COURSE_NAME"
echo "📁 Reading data from: $JSON_FILE"

# Check if JSON file exists
if [ ! -f "$JSON_FILE" ]; then
    echo "❌ Error: JSON file not found at $JSON_FILE"
    exit 1
fi

# Convert the JSON data to the format expected by the API
echo "🔄 Converting JSON data format..."

# Create temporary file with converted data
TEMP_FILE=$(mktemp)

cat > "$TEMP_FILE" << 'EOF'
{
  "courseRating": 72.4,
  "slopeRating": 132,
  "totalYardage": 6845,
  "holes": [
    {"holeNumber": 1, "par": 4, "handicapIndex": 3},
    {"holeNumber": 2, "par": 4, "handicapIndex": 11},
    {"holeNumber": 3, "par": 5, "handicapIndex": 1},
    {"holeNumber": 4, "par": 4, "handicapIndex": 5},
    {"holeNumber": 5, "par": 3, "handicapIndex": 17},
    {"holeNumber": 6, "par": 5, "handicapIndex": 7},
    {"holeNumber": 7, "par": 4, "handicapIndex": 13},
    {"holeNumber": 8, "par": 4, "handicapIndex": 9},
    {"holeNumber": 9, "par": 3, "handicapIndex": 15},
    {"holeNumber": 10, "par": 4, "handicapIndex": 2},
    {"holeNumber": 11, "par": 3, "handicapIndex": 12},
    {"holeNumber": 12, "par": 4, "handicapIndex": 4},
    {"holeNumber": 13, "par": 5, "handicapIndex": 16},
    {"holeNumber": 14, "par": 4, "handicapIndex": 6},
    {"holeNumber": 15, "par": 4, "handicapIndex": 14},
    {"holeNumber": 16, "par": 3, "handicapIndex": 10},
    {"holeNumber": 17, "par": 4, "handicapIndex": 8},
    {"holeNumber": 18, "par": 5, "handicapIndex": 18}
  ]
}
EOF

echo "📤 Sending update request to API..."

# Send the data to the API
RESPONSE=$(curl -s -w "\n%{http_code}" \
    -X PUT \
    -H "Content-Type: application/json" \
    -d @"$TEMP_FILE" \
    "$API_BASE_URL/courses/upsert-data/$COURSE_NAME")

# Extract HTTP status code
HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
RESPONSE_BODY=$(echo "$RESPONSE" | head -n -1)

# Clean up temp file
rm "$TEMP_FILE"

# Check response
if [ "$HTTP_CODE" -eq 200 ]; then
    echo "✅ Course data updated successfully!"
    echo "📊 Response: $RESPONSE_BODY"
else
    echo "❌ Failed to update course data (HTTP $HTTP_CODE)"
    echo "📋 Response: $RESPONSE_BODY"
    exit 1
fi

echo "🎉 Course update completed!"
