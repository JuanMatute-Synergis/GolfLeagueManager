#!/bin/bash

echo "==================================="
echo "Golf League Score Import - JSON"
echo "==================================="

JSON_FILE="../data/Scores.json"

if [ ! -f "$JSON_FILE" ]; then
    echo "❌ JSON file not found: $JSON_FILE"
    exit 1
fi

echo "📁 Reading JSON file: $JSON_FILE"

echo "� Creating JSON payload wrapper..."
# Read the JSON file and escape it properly for inclusion in another JSON
JSON_CONTENT=$(cat "$JSON_FILE" | jq -Rs .)

# Create the wrapper JSON with proper escaping
cat > /tmp/json_import_payload.json << EOF
{
  "jsonContent": $JSON_CONTENT
}
EOF

echo "�🚀 Sending JSON import request to API..."
echo "⏳ This may take a while as it processes all players and rounds..."

curl -X POST "http://localhost:5505/api/import/json-scores" \
  -H "Content-Type: application/json" \
  --data-binary @/tmp/json_import_payload.json \
  --max-time 300 \
  -w "\nHTTP Status: %{http_code}\nTime: %{time_total}s\n"

echo -e "\n✅ Import completed!"

# Clean up
rm -f /tmp/json_import_payload.json
