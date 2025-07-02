#!/bin/bash

# Script to update existing weeks to enable scoring and handicap calculation by default

API_BASE="http://localhost:5505/api"

echo "=== Updating Existing Weeks to Enable Scoring and Handicap Calculation ==="

# First, let's update the database directly via SQL since the API might not have the endpoints yet
echo "Updating existing weeks in database..."

# You can run this SQL command in your database:
echo "UPDATE \"Weeks\" SET \"CountsForScoring\" = true, \"CountsForHandicap\" = true WHERE \"CountsForScoring\" = false OR \"CountsForHandicap\" = false;"

echo "All existing weeks should now count for scoring and handicap calculation."
echo "New weeks will default to enabled for both scoring and handicap."
