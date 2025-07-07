#!/bin/bash

# Update PlayerSeasonRecords InitialAverageScore to match reference data
# Database connection info from docker-compose.yml
DB_HOST="localhost"
DB_PORT="5432"
DB_NAME="golfdb_htlyons"
DB_USER="golfuser"
DB_PASSWORD="golfpassword"

echo "Updating PlayerSeasonRecords InitialAverageScore values to match reference data..."

# Execute SQL updates using psql
PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME << 'EOF'

-- Update InitialAverageScore for all players based on reference data
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 41.17
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'George Hutson';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 41.43
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Jeff Dilcher';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 42.30
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Bill Stein';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 42.75
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Alex Peck';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 43.22
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Tim Seyler';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 43.52
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Kevin Kelhart';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 43.57
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Joe Mahachanh';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 43.60
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'John Perry';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 43.60
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Carl Hardner';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 44.10
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Jay Sullivan';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 44.63
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Stu Silfies';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 44.68
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Steve Bedek';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 44.79
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Curt Saeger';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 44.85
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Lou Gabrielle';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 44.97
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Frank Frankenfield';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 44.99
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Kenny Palladino';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 45.24
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Matt Speth';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 46.45
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Jim Eck';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 46.58
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Kevin Kelhart JR';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 47.59
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Steve Hampton';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 47.85
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Bob Gross';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 47.95
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Juan Matute';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 47.95
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Matt Donahue';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 47.95
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Danny Washurn';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 48.42
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Ray Ballinger';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 50.71
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Rich Hart';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 51.43
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Mike Schaefer';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 53.32
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Steve Kerns';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 55.20
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Steve Filipovits';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 55.37
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Andrew Kerns';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 60.50
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Tom Haeusler';

UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 60.50
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."Name" = 'Jax Haeusler';

-- Show updated records to verify
SELECT p."Name", psr."InitialAverageScore" 
FROM "PlayerSeasonRecords" psr
JOIN "Players" p ON psr."PlayerId" = p."Id"
ORDER BY psr."InitialAverageScore";

EOF

echo "Database update completed!"
