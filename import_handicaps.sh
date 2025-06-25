#!/bin/bash

# Script to import Week 5 handicap data as Session 2 handicaps
# Database connection info from docker-compose.yml
# Host: localhost, Port: 5432, Database: golfdb, User: golfuser, Password: golfpassword

echo "=== Golf League Handicap Import Script ==="
echo "This script will import Week 5 handicap data as Session 2 handicaps (starting week 8)"
echo ""

# Check if psql is available
if ! command -v psql &> /dev/null; then
    echo "Error: psql command not found. Please install PostgreSQL client tools."
    exit 1
fi

# Set database connection variables
export PGHOST=localhost
export PGPORT=5432
export PGDATABASE=golfdb
export PGUSER=golfuser
export PGPASSWORD=golfpassword

echo "Step 1: Checking database connection..."
psql -c "SELECT 1;" > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo "Error: Cannot connect to database. Make sure PostgreSQL is running and docker-compose is up."
    exit 1
fi
echo "✓ Database connection successful"

echo ""
echo "Step 2: Getting Season ID for 2025..."
SEASON_ID=$(psql -t -c "SELECT \"Id\" FROM \"Seasons\" WHERE \"Name\" ILIKE '%2025%' LIMIT 1;" | tr -d ' ' | tr -d '\n')

if [ -z "$SEASON_ID" ]; then
    echo "Error: Could not find 2025 season. Available seasons:"
    psql -c "SELECT \"Id\", \"Name\", \"StartDate\" FROM \"Seasons\" ORDER BY \"StartDate\" DESC;"
    echo "Please manually set SEASON_ID variable in this script."
    exit 1
fi

echo "✓ Found Season ID: $SEASON_ID"

echo ""
echo "Step 3: Checking for existing session handicaps for week 8..."
EXISTING_COUNT=$(psql -t -c "SELECT COUNT(*) FROM \"PlayerSessionHandicaps\" WHERE \"SeasonId\" = '$SEASON_ID' AND \"SessionStartWeekNumber\" = 8;" | tr -d ' ')

if [ "$EXISTING_COUNT" -gt 0 ]; then
    echo "Warning: Found $EXISTING_COUNT existing session handicaps for week 8."
    echo "Do you want to delete them and reimport? (y/N)"
    read -r response
    if [[ "$response" =~ ^[Yy]$ ]]; then
        echo "Deleting existing session handicaps..."
        psql -c "DELETE FROM \"PlayerSessionHandicaps\" WHERE \"SeasonId\" = '$SEASON_ID' AND \"SessionStartWeekNumber\" = 8;"
        echo "✓ Existing records deleted"
    else
        echo "Exiting without changes."
        exit 0
    fi
fi

echo ""
echo "Step 4: Checking player name matches..."
psql -c "
WITH handicap_data AS (
  SELECT 'George Hutson' as player_name, 5 as handicap
  UNION ALL SELECT 'Jeff Dilcher', 5
  UNION ALL SELECT 'Bill Stein', 5  
  UNION ALL SELECT 'Alex Peck', 6
  UNION ALL SELECT 'Tim Seyler', 6
  UNION ALL SELECT 'Kevin Kelhart', 6
  UNION ALL SELECT 'Joe Mahachanh', 6
  UNION ALL SELECT 'John Perry', 5
  UNION ALL SELECT 'Carl Hardner', 6
  UNION ALL SELECT 'Jay Sullivan', 6
  UNION ALL SELECT 'Stu Silfies', 7
  UNION ALL SELECT 'Steve Bedek', 7
  UNION ALL SELECT 'Curt Saeger', 6
  UNION ALL SELECT 'Lou Gabrielle', 7
  UNION ALL SELECT 'Frank Frankenfield', 7
  UNION ALL SELECT 'Kenny Palladino', 6
  UNION ALL SELECT 'Matt Speth', 6
  UNION ALL SELECT 'Jim Eck', 7
  UNION ALL SELECT 'Kevin Kelhart JR', 9
  UNION ALL SELECT 'Steve Hampton', 8
  UNION ALL SELECT 'Bob Gross', 8
  UNION ALL SELECT 'Juan Matute', 8
  UNION ALL SELECT 'Matt Donahue', 9
  UNION ALL SELECT 'Danny Washurn', 9
  UNION ALL SELECT 'Ray Ballinger', 8
  UNION ALL SELECT 'Rich Hart', 11
  UNION ALL SELECT 'Mike Schaefer', 11
  UNION ALL SELECT 'Steve Kerns', 13
  UNION ALL SELECT 'Steve Filipovits', 13
  UNION ALL SELECT 'Andrew Kerns', 14
  UNION ALL SELECT 'Tom Haeusler', 16
  UNION ALL SELECT 'Jax Haeusler', 15
),
matched_players AS (
  SELECT 
    hd.player_name,
    hd.handicap,
    p.\"Id\" as player_id,
    p.\"FirstName\" || ' ' || p.\"LastName\" as full_name
  FROM handicap_data hd
  LEFT JOIN \"Players\" p ON (
    (p.\"FirstName\" || ' ' || p.\"LastName\") = hd.player_name
    OR
    (p.\"FirstName\" || ' ' || p.\"LastName\") ILIKE '%' || hd.player_name || '%'
    OR
    hd.player_name ILIKE '%' || (p.\"FirstName\" || ' ' || p.\"LastName\") || '%'
  )
)
SELECT 
  player_name as \"Handicap Data Name\",
  handicap as \"Handicap\",
  full_name as \"Database Player Name\",
  CASE WHEN player_id IS NULL THEN 'NO MATCH' ELSE 'MATCHED' END as \"Status\"
FROM matched_players
ORDER BY CASE WHEN player_id IS NULL THEN 1 ELSE 0 END, player_name;
"

echo ""
echo "Please review the player matches above. Continue with import? (y/N)"
read -r response
if [[ ! "$response" =~ ^[Yy]$ ]]; then
    echo "Import cancelled."
    exit 0
fi

echo ""
echo "Step 5: Importing handicap data..."
IMPORT_RESULT=$(psql -c "
INSERT INTO \"PlayerSessionHandicaps\" (
  \"Id\",
  \"PlayerId\", 
  \"SeasonId\",
  \"SessionStartWeekNumber\",
  \"SessionInitialHandicap\",
  \"CreatedDate\"
)
SELECT 
  gen_random_uuid() as \"Id\",
  p.\"Id\" as \"PlayerId\",
  '$SEASON_ID'::uuid as \"SeasonId\",
  8 as \"SessionStartWeekNumber\",
  hd.handicap as \"SessionInitialHandicap\",
  NOW() as \"CreatedDate\"
FROM (
  SELECT 'George Hutson' as player_name, 5 as handicap
  UNION ALL SELECT 'Jeff Dilcher', 5
  UNION ALL SELECT 'Bill Stein', 5  
  UNION ALL SELECT 'Alex Peck', 6
  UNION ALL SELECT 'Tim Seyler', 6
  UNION ALL SELECT 'Kevin Kelhart', 6
  UNION ALL SELECT 'Joe Mahachanh', 6
  UNION ALL SELECT 'John Perry', 5
  UNION ALL SELECT 'Carl Hardner', 6
  UNION ALL SELECT 'Jay Sullivan', 6
  UNION ALL SELECT 'Stu Silfies', 7
  UNION ALL SELECT 'Steve Bedek', 7
  UNION ALL SELECT 'Curt Saeger', 6
  UNION ALL SELECT 'Lou Gabrielle', 7
  UNION ALL SELECT 'Frank Frankenfield', 7
  UNION ALL SELECT 'Kenny Palladino', 6
  UNION ALL SELECT 'Matt Speth', 6
  UNION ALL SELECT 'Jim Eck', 7
  UNION ALL SELECT 'Kevin Kelhart JR', 9
  UNION ALL SELECT 'Steve Hampton', 8
  UNION ALL SELECT 'Bob Gross', 8
  UNION ALL SELECT 'Juan Matute', 8
  UNION ALL SELECT 'Matt Donahue', 9
  UNION ALL SELECT 'Danny Washurn', 9
  UNION ALL SELECT 'Ray Ballinger', 8
  UNION ALL SELECT 'Rich Hart', 11
  UNION ALL SELECT 'Mike Schaefer', 11
  UNION ALL SELECT 'Steve Kerns', 13
  UNION ALL SELECT 'Steve Filipovits', 13
  UNION ALL SELECT 'Andrew Kerns', 14
  UNION ALL SELECT 'Tom Haeusler', 16
  UNION ALL SELECT 'Jax Haeusler', 15
) hd
INNER JOIN \"Players\" p ON (
  (p.\"FirstName\" || ' ' || p.\"LastName\") = hd.player_name
  OR
  (p.\"FirstName\" || ' ' || p.\"LastName\") ILIKE '%' || hd.player_name || '%'
  OR
  hd.player_name ILIKE '%' || (p.\"FirstName\" || ' ' || p.\"LastName\") || '%'
);
")

echo "✓ Import completed"

echo ""
echo "Step 6: Verification - Showing imported records..."
psql -c "
SELECT 
  p.\"FirstName\" || ' ' || p.\"LastName\" as \"Player Name\",
  psh.\"SessionInitialHandicap\" as \"Session Handicap\",
  psh.\"SessionStartWeekNumber\" as \"Session Start Week\",
  psh.\"CreatedDate\"
FROM \"PlayerSessionHandicaps\" psh
INNER JOIN \"Players\" p ON p.\"Id\" = psh.\"PlayerId\"
WHERE psh.\"SeasonId\" = '$SEASON_ID' AND psh.\"SessionStartWeekNumber\" = 8
ORDER BY p.\"LastName\", p.\"FirstName\";
"

FINAL_COUNT=$(psql -t -c "SELECT COUNT(*) FROM \"PlayerSessionHandicaps\" WHERE \"SeasonId\" = '$SEASON_ID' AND \"SessionStartWeekNumber\" = 8;" | tr -d ' ')
echo ""
echo "=== Import Summary ==="
echo "Total records imported: $FINAL_COUNT"
echo "Season ID: $SEASON_ID"
echo "Session Start Week: 8"
echo "Import completed successfully!"
