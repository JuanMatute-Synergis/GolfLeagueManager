-- Manual SQL Import for Week 5 Handicap Data as Session 2 Handicaps
-- Connection: psql -h localhost -p 5432 -d golfdb -U golfuser
-- Password: golfpassword

-- STEP 1: Find your season ID (run this first)
SELECT "Id", "Name", "StartDate", "EndDate" 
FROM "Seasons" 
ORDER BY "StartDate" DESC;

-- STEP 2: Check current players (to verify name matching)
SELECT "Id", "FirstName", "LastName", "CurrentHandicap" 
FROM "Players" 
ORDER BY "LastName", "FirstName";

-- STEP 3: Preview the data matching (run this to check before inserting)
WITH handicap_data AS (
  -- All players from Week 5 document
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
)
SELECT 
  hd.player_name as "Import Name",
  hd.handicap as "Handicap",
  p."FirstName" || ' ' || p."LastName" as "Database Name",
  p."Id" as "Player ID",
  CASE WHEN p."Id" IS NULL THEN '❌ NO MATCH' ELSE '✅ MATCHED' END as "Status"
FROM handicap_data hd
LEFT JOIN "Players" p ON (
  (p."FirstName" || ' ' || p."LastName") = hd.player_name
  OR (p."FirstName" || ' ' || p."LastName") ILIKE '%' || hd.player_name || '%'
  OR hd.player_name ILIKE '%' || (p."FirstName" || ' ' || p."LastName") || '%'
)
ORDER BY CASE WHEN p."Id" IS NULL THEN 1 ELSE 0 END, hd.player_name;

-- STEP 4: Replace 'YOUR_SEASON_ID_HERE' with actual season ID from STEP 1 and run this insert
/*
INSERT INTO "PlayerSessionHandicaps" (
  "Id",
  "PlayerId", 
  "SeasonId",
  "SessionStartWeekNumber",
  "SessionInitialHandicap",
  "CreatedDate"
)
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
)
SELECT 
  gen_random_uuid() as "Id",
  p."Id" as "PlayerId",
  'YOUR_SEASON_ID_HERE'::uuid as "SeasonId", -- ⚠️  REPLACE THIS
  8 as "SessionStartWeekNumber", -- Session 2 starts at week 8
  hd.handicap as "SessionInitialHandicap",
  NOW() as "CreatedDate"
FROM handicap_data hd
INNER JOIN "Players" p ON (
  (p."FirstName" || ' ' || p."LastName") = hd.player_name
  OR (p."FirstName" || ' ' || p."LastName") ILIKE '%' || hd.player_name || '%'
  OR hd.player_name ILIKE '%' || (p."FirstName" || ' ' || p."LastName") || '%'
)
WHERE NOT EXISTS (
  SELECT 1 FROM "PlayerSessionHandicaps" psh 
  WHERE psh."PlayerId" = p."Id" 
  AND psh."SeasonId" = 'YOUR_SEASON_ID_HERE'::uuid  -- ⚠️  REPLACE THIS TOO
  AND psh."SessionStartWeekNumber" = 8
);
*/

-- STEP 5: Verify the import (replace season ID)
/*
SELECT 
  p."FirstName" || ' ' || p."LastName" as "Player Name",
  psh."SessionInitialHandicap" as "Session Handicap",
  psh."SessionStartWeekNumber" as "Session Week",
  psh."CreatedDate"
FROM "PlayerSessionHandicaps" psh
INNER JOIN "Players" p ON p."Id" = psh."PlayerId"
WHERE psh."SeasonId" = 'YOUR_SEASON_ID_HERE'::uuid  -- ⚠️  REPLACE THIS
  AND psh."SessionStartWeekNumber" = 8
ORDER BY p."LastName", p."FirstName";
*/
