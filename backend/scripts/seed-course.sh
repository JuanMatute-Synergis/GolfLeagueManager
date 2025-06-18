#!/bin/bash

echo "Starting data seeding..."

# Seed the Allentown Municipal Golf Course
echo "Seeding Allentown Municipal Golf Course..."
curl -X POST http://localhost:5274/api/courses/seed-allentown \
  -H "Content-Type: application/json" \
  -w "\nHTTP Status: %{http_code}\n"

echo ""

# Seed player handicap data
echo "Seeding player handicap data..."
curl -X POST http://localhost:5274/api/courses/seed-players \
  -H "Content-Type: application/json" \
  -w "\nHTTP Status: %{http_code}\n"

echo ""
echo "Data seeding completed."
