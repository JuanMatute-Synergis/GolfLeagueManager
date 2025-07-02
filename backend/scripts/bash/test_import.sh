#!/bin/bash

# Simple test of the import API
echo "Testing Golf League Score Import API..."

# Create a simple test JSON payload
cat > /tmp/import_test.json << 'EOF'
{
  "csvContent": "Player,Hole_1,Hole_2,Hole_3,Hole_4,Hole_5,Hole_6,Hole_7,Hole_8,Hole_9,Total_Score,Week_Number,Front_or_Back,Hole_10,Hole_11,Hole_12,Hole_13,Hole_14,Hole_15,Hole_16,Hole_17,Hole_18\nGeorge Hutson,5.0,4.0,4.0,6.0,2.0,5.0,4.0,5.0,4.0,39.0,1,Front,,,,,,,,,"
}
EOF

echo "Sending request to import API..."
curl -X POST "http://localhost:5505/api/import/scores" \
  -H "Content-Type: application/json" \
  -d @/tmp/import_test.json \
  --max-time 30

echo -e "\n\nDone!"
