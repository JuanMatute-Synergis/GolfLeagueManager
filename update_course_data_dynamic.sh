#!/bin/bash

# Dynamic script to update course data from allentown-hcp-data.json
# Usage: ./update_course_data_dynamic.sh

# Configuration
API_BASE_URL="http://localhost:5274/api"
COURSE_NAME="Allentown Municipal"
JSON_FILE="backend/scripts/data/allentown-hcp-data.json"

echo "🏌️  Updating course data for: $COURSE_NAME"
echo "📁 Reading data from: $JSON_FILE"

# Check if JSON file exists
if [ ! -f "$JSON_FILE" ]; then
    echo "❌ Error: JSON file not found at $JSON_FILE"
    exit 1
fi

# Check if jq is available
if ! command -v jq &> /dev/null; then
    echo "❌ Error: jq is required but not installed. Please install jq first."
    echo "   On macOS: brew install jq"
    exit 1
fi

echo "🔄 Converting JSON data format..."

# Create temporary file with converted data using jq
TEMP_FILE=$(mktemp)

jq '{
  courseRating: .course.rating,
  slopeRating: .course.slope,
  totalYardage: .course.length,
  holes: [
    .par_data.holes[] as $par |
    .handicap_data.mens.holes[] as $hcp |
    if $par.hole == $hcp.hole then
      {
        holeNumber: $par.hole,
        par: $par.par,
        handicapIndex: $hcp.handicap
      }
    else empty end
  ]
}' "$JSON_FILE" > "$TEMP_FILE"

echo "📤 Sending update request to API..."

# Send the data to the API
RESPONSE=$(curl -s -w "%{http_code}" \
    -X PUT \
    -H "Content-Type: application/json" \
    -d @"$TEMP_FILE" \
    "$API_BASE_URL/courses/upsert-data/$COURSE_NAME")

# Extract HTTP status code (last 3 characters)
HTTP_CODE="${RESPONSE: -3}"
RESPONSE_BODY="${RESPONSE%???}"

# Clean up temp file
rm "$TEMP_FILE"

# Check response
if [ "$HTTP_CODE" -eq 200 ]; then
    echo "✅ Course data updated successfully!"
    echo "📊 Updated course info:"
    echo "$RESPONSE_BODY" | jq '.'
else
    echo "❌ Failed to update course data (HTTP $HTTP_CODE)"
    echo "📋 Response: $RESPONSE_BODY"
    exit 1
fi

echo "🎉 Course update completed!"
