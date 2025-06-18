#!/bin/bash

echo "=========================================="
echo "Golf League Score Import - Corrected CSV"
echo "=========================================="

CSV_FILE="/Users/juanmatute/Sources/GolfLeagueManager/backend/Business/Scores.csv"

if [ ! -f "$CSV_FILE" ]; then
    echo "❌ CSV file not found: $CSV_FILE"
    exit 1
fi

echo "📁 Reading corrected CSV file: $CSV_FILE"
CSV_CONTENT=$(cat "$CSV_FILE" | sed 's/"/\\"/g' | awk '{printf "%s\\n", $0}')

echo "📊 Creating JSON payload..."
cat > /tmp/corrected_import.json << EOF
{
  "csvContent": "$CSV_CONTENT"
}
EOF

echo "🚀 Sending corrected import request to API..."
echo "⏳ This may take a while as it processes all players and weeks..."

curl -X POST "http://localhost:5274/api/import/formatted-scores" \
  -H "Content-Type: application/json" \
  -d @/tmp/corrected_import.json \
  --max-time 300 \
  -w "\nHTTP Status: %{http_code}\nTime: %{time_total}s\n"

echo -e "\n✅ Import completed!"

# Clean up
rm -f /tmp/corrected_import.json
