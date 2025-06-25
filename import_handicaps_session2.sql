-- SQL Script to Import Week 5 Handicap Data as Session 2 Handicaps (Starting Week 8)
-- Database: PostgreSQL
-- Connection: localhost:5432, database: golfdb, user: golfuser, password: golfpassword

-- First, let's check the current seasons and players to get the correct IDs
-- Run these queries first to understand the data structure:

-- Check available seasons
SELECT "Id", "Name", "StartDate", "EndDate" FROM "Seasons" ORDER BY "StartDate" DESC;

-- Check players (you'll need to match names from the JSON to player IDs)
SELECT "Id", "FirstName", "LastName", "CurrentHandicap" FROM "Players" ORDER BY "LastName", "FirstName";

-- Check existing weeks for session start markers
SELECT "Id", "WeekNumber", "SeasonId", "SessionStart", "Date" FROM "Weeks" 
WHERE "SessionStart" = true 
ORDER BY "WeekNumber";

-- ========================================
-- MAIN IMPORT SCRIPT
-- ========================================
-- NOTE: You'll need to replace the @SeasonId variable with the actual Season ID from your database
-- and ensure player names match exactly with your Player table

-- Define variables (replace with actual values from your database)
-- DECLARE @SeasonId UUID; -- Replace with actual season ID for 2025
-- SET @SeasonId = 'your-season-id-here'; -- Replace this with actual GUID

-- For PostgreSQL, we'll use a WITH clause to define our data
WITH handicap_data AS (
  -- Flight 1
  SELECT 'George Hutson' as player_name, 5 as handicap
  UNION ALL SELECT 'Jeff Dilcher', 5
  UNION ALL SELECT 'Bill Stein', 5  
  UNION ALL SELECT 'Alex Peck', 6
  UNION ALL SELECT 'Tim Seyler', 6
  UNION ALL SELECT 'Kevin Kelhart', 6
  UNION ALL SELECT 'Joe Mahachanh', 6
  UNION ALL SELECT 'John Perry', 5
  
  -- Flight 2
  UNION ALL SELECT 'Carl Hardner', 6
  UNION ALL SELECT 'Jay Sullivan', 6
  UNION ALL SELECT 'Stu Silfies', 7
  UNION ALL SELECT 'Steve Bedek', 7
  UNION ALL SELECT 'Curt Saeger', 6
  UNION ALL SELECT 'Lou Gabrielle', 7
  UNION ALL SELECT 'Frank Frankenfield', 7
  UNION ALL SELECT 'Kenny Palladino', 6
  
  -- Flight 3
  UNION ALL SELECT 'Matt Speth', 6
  UNION ALL SELECT 'Jim Eck', 7
  UNION ALL SELECT 'Kevin Kelhart JR', 9
  UNION ALL SELECT 'Steve Hampton', 8
  UNION ALL SELECT 'Bob Gross', 8
  UNION ALL SELECT 'Juan Matute', 8
  UNION ALL SELECT 'Matt Donahue', 9
  UNION ALL SELECT 'Danny Washurn', 9
  
  -- Flight 4
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
    p."Id" as player_id,
    p."FirstName" || ' ' || p."LastName" as full_name
  FROM handicap_data hd
  LEFT JOIN "Players" p ON (
    -- Try exact match first
    (p."FirstName" || ' ' || p."LastName") = hd.player_name
    OR
    -- Try partial matches for common variations
    (p."FirstName" || ' ' || p."LastName") ILIKE '%' || hd.player_name || '%'
    OR
    hd.player_name ILIKE '%' || (p."FirstName" || ' ' || p."LastName") || '%'
  )
)
SELECT 
  player_name,
  handicap,
  player_id,
  full_name,
  CASE WHEN player_id IS NULL THEN 'NO MATCH FOUND' ELSE 'MATCHED' END as match_status
FROM matched_players
ORDER BY match_status, player_name;

-- ========================================
-- ACTUAL INSERT STATEMENTS
-- ========================================
-- Run this section AFTER confirming the matches above and getting the correct season ID

/*
-- Step 1: Replace 'YOUR_SEASON_ID_HERE' with the actual season ID
-- Step 2: Uncomment and run this INSERT statement

INSERT INTO "PlayerSessionHandicaps" (
  "Id",
  "PlayerId", 
  "SeasonId",
  "SessionStartWeekNumber",
  "SessionInitialHandicap",
  "CreatedDate"
)
SELECT 
  gen_random_uuid() as "Id",
  p."Id" as "PlayerId",
  'YOUR_SEASON_ID_HERE'::uuid as "SeasonId", -- Replace with actual season ID
  8 as "SessionStartWeekNumber", -- Session 2 starts at week 8
  hd.handicap as "SessionInitialHandicap",
  NOW() as "CreatedDate"
FROM (
  -- Flight 1
  SELECT 'George Hutson' as player_name, 5 as handicap
  UNION ALL SELECT 'Jeff Dilcher', 5
  UNION ALL SELECT 'Bill Stein', 5  
  UNION ALL SELECT 'Alex Peck', 6
  UNION ALL SELECT 'Tim Seyler', 6
  UNION ALL SELECT 'Kevin Kelhart', 6
  UNION ALL SELECT 'Joe Mahachanh', 6
  UNION ALL SELECT 'John Perry', 5
  
  -- Flight 2
  UNION ALL SELECT 'Carl Hardner', 6
  UNION ALL SELECT 'Jay Sullivan', 6
  UNION ALL SELECT 'Stu Silfies', 7
  UNION ALL SELECT 'Steve Bedek', 7
  UNION ALL SELECT 'Curt Saeger', 6
  UNION ALL SELECT 'Lou Gabrielle', 7
  UNION ALL SELECT 'Frank Frankenfield', 7
  UNION ALL SELECT 'Kenny Palladino', 6
  
  -- Flight 3
  UNION ALL SELECT 'Matt Speth', 6
  UNION ALL SELECT 'Jim Eck', 7
  UNION ALL SELECT 'Kevin Kelhart JR', 9
  UNION ALL SELECT 'Steve Hampton', 8
  UNION ALL SELECT 'Bob Gross', 8
  UNION ALL SELECT 'Juan Matute', 8
  UNION ALL SELECT 'Matt Donahue', 9
  UNION ALL SELECT 'Danny Washurn', 9
  
  -- Flight 4
  UNION ALL SELECT 'Ray Ballinger', 8
  UNION ALL SELECT 'Rich Hart', 11
  UNION ALL SELECT 'Mike Schaefer', 11
  UNION ALL SELECT 'Steve Kerns', 13
  UNION ALL SELECT 'Steve Filipovits', 13
  UNION ALL SELECT 'Andrew Kerns', 14
  UNION ALL SELECT 'Tom Haeusler', 16
  UNION ALL SELECT 'Jax Haeusler', 15
) hd
INNER JOIN "Players" p ON (
  (p."FirstName" || ' ' || p."LastName") = hd.player_name
  OR
  (p."FirstName" || ' ' || p."LastName") ILIKE '%' || hd.player_name || '%'
  OR
  hd.player_name ILIKE '%' || (p."FirstName" || ' ' || p."LastName") || '%'
)
-- Avoid duplicates
WHERE NOT EXISTS (
  SELECT 1 FROM "PlayerSessionHandicaps" psh 
  WHERE psh."PlayerId" = p."Id" 
  AND psh."SeasonId" = 'YOUR_SEASON_ID_HERE'::uuid 
  AND psh."SessionStartWeekNumber" = 8
);
*/

-- ========================================
-- VERIFICATION QUERIES
-- ========================================
-- Run these after the insert to verify the data was imported correctly

/*
-- Check how many records were inserted
SELECT COUNT(*) as total_session_handicaps
FROM "PlayerSessionHandicaps" 
WHERE "SessionStartWeekNumber" = 8;

-- Verify the data with player names
SELECT 
  p."FirstName" || ' ' || p."LastName" as player_name,
  psh."SessionInitialHandicap",
  psh."SessionStartWeekNumber",
  psh."CreatedDate"
FROM "PlayerSessionHandicaps" psh
INNER JOIN "Players" p ON p."Id" = psh."PlayerId"
WHERE psh."SessionStartWeekNumber" = 8
ORDER BY p."LastName", p."FirstName";
*/
