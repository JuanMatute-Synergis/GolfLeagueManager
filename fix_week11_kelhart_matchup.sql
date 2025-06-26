-- Fix the Kevin Kelhart JR vs Steve Hampton matchup for Week 11
-- This script fixes the missing matchup that failed in the previous script

DO $$
DECLARE
    week_id UUID;
    season_id UUID;
    kevin_jr_id UUID;
    steve_h_id UUID;
BEGIN
    -- Find the Week 11 ID for the current season
    SELECT "Id" INTO season_id FROM "Seasons" WHERE "Year" = 2025 LIMIT 1;
    SELECT "Id" INTO week_id FROM "Weeks" WHERE "WeekNumber" = 11 AND "SeasonId" = season_id;
    
    -- Get specific player IDs
    SELECT "Id" INTO kevin_jr_id FROM "Players" WHERE "FirstName" = 'Kevin' AND "LastName" = 'Kelhart JR';
    SELECT "Id" INTO steve_h_id FROM "Players" WHERE "FirstName" = 'Steve' AND "LastName" = 'Hampton';
    
    RAISE NOTICE 'Kevin Kelhart JR ID: %', kevin_jr_id;
    RAISE NOTICE 'Steve Hampton ID: %', steve_h_id;
    
    -- Insert the missing matchup
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    VALUES (
        gen_random_uuid(),
        week_id,
        kevin_jr_id,
        steve_h_id
    );
    
    RAISE NOTICE 'Fixed Kevin Kelhart JR vs Steve Hampton matchup';
    
END $$;

-- Verify all Week 11 matchups
SELECT 
    w."WeekNumber",
    CONCAT(pa."FirstName", ' ', pa."LastName") AS "PlayerA",
    CONCAT(pb."FirstName", ' ', pb."LastName") AS "PlayerB"
FROM "Matchups" m
JOIN "Weeks" w ON m."WeekId" = w."Id"
JOIN "Players" pa ON m."PlayerAId" = pa."Id"
JOIN "Players" pb ON m."PlayerBId" = pb."Id"
WHERE w."WeekNumber" = 11 
ORDER BY pa."LastName";
