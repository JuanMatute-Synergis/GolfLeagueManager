#!/bin/bash

echo "=========================================="
echo "Debug: George Hutson Score Import Only"
echo "=========================================="

CSV_FILE="/Users/juanmatute/Sources/GolfLeagueManager/debug_george_only.csv"

if [ ! -f "$CSV_FILE" ]; then
    echo "❌ CSV file not found: $CSV_FILE"
    exit 1
fi

echo "📁 Reading George Hutson CSV file: $CSV_FILE"
echo "File contents:"
cat "$CSV_FILE"
echo ""

CSV_CONTENT=$(cat "$CSV_FILE" | sed 's/"/\\"/g' | awk '{printf "%s\\n", $0}')

echo "📊 Creating JSON payload..."
cat > /tmp/george_import.json << EOF
{
  "csvContent": "$CSV_CONTENT"
}
EOF

echo "JSON payload:"
cat /tmp/george_import.json
echo ""

echo "🚀 Sending George Hutson import request to API..."

curl -X POST "http://localhost:5505/api/import/formatted-scores" \
  -H "Content-Type: application/json" \
  -d @/tmp/george_import.json \
  --max-time 300 \
  -v \
  -w "\nHTTP Status: %{http_code}\nTime: %{time_total}s\n"

echo -e "\n✅ Import completed!"

# Clean up
rm -f /tmp/george_import.json
