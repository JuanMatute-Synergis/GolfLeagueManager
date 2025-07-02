#!/bin/bash

echo "🗑️  Database Cleanup - Delete All Scores"
echo "========================================"

echo "⚠️  WARNING: This will delete ALL hole scores and reset ALL matchups!"
echo "⏳ Sending cleanup request to API..."

curl -X DELETE "http://localhost:5505/api/cleanup/all-scores" \
  -H "Content-Type: application/json" \
  --max-time 60 \
  -w "\nHTTP Status: %{http_code}\nTime: %{time_total}s\n"

echo -e "\n✅ Cleanup completed!"
