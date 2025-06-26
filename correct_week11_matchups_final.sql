-- Fix Week 11 matchups by removing duplicate and correcting pairings
-- Based on the conversation, the correct Week 11 matchups should be:

DO $$
DECLARE
    week_id UUID;
    season_id UUID;
BEGIN
    -- Find the Week 11 ID for the current season
    SELECT "Id" INTO season_id FROM "Seasons" WHERE "Year" = 2025 LIMIT 1;
    SELECT "Id" INTO week_id FROM "Weeks" WHERE "WeekNumber" = 11 AND "SeasonId" = season_id;
    
    -- Delete all existing Week 11 matchups to start fresh
    DELETE FROM "HoleScores" 
    WHERE "MatchupId" IN (SELECT "Id" FROM "Matchups" WHERE "WeekId" = week_id);
    
    DELETE FROM "Matchups" 
    WHERE "WeekId" = week_id;
    
    RAISE NOTICE 'Cleared all Week 11 matchups';
    
    -- Insert the correct 16 matchups for Week 11
    
    -- Flight 1 (4 matchups)
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'George' AND "LastName" = 'Hutson'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Kevin' AND "LastName" = 'Kelhart' AND "LastName" != 'Kelhart JR');
    
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Jeff' AND "LastName" = 'Dilcher'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'John' AND "LastName" = 'Perry');
    
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Bill' AND "LastName" = 'Stein'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Alex' AND "LastName" = 'Peck');
    
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Tim' AND "LastName" = 'Seyler'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Joe' AND "LastName" = 'Mahachanh');
    
    -- Flight 2 (4 matchups)
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Carl' AND "LastName" = 'Hardner'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Lou' AND "LastName" = 'Gabrielle');
    
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Jay' AND "LastName" = 'Sullivan'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Kenny' AND "LastName" = 'Palladino');
    
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Stu' AND "LastName" = 'Silfies'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Steve' AND "LastName" = 'Bedek');
    
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Curt' AND "LastName" = 'Saeger'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Frank' AND "LastName" = 'Frankenfield');
    
    -- Flight 3 (4 matchups)
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Matt' AND "LastName" = 'Speth'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Juan' AND "LastName" = 'Matute');
    
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Jim' AND "LastName" = 'Eck'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Danny' AND "LastName" = 'Washurn');
    
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Kevin' AND "LastName" = 'Kelhart JR'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Steve' AND "LastName" = 'Hampton');
    
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Bob' AND "LastName" = 'Gross'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Matt' AND "LastName" = 'Donahue');
    
    -- Flight 4 (4 matchups)
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Ray' AND "LastName" = 'Ballinger'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Andrew' AND "LastName" = 'Kerns');
    
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Rich' AND "LastName" = 'Hart'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Jax' AND "LastName" = 'Haeusler');
    
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Mike' AND "LastName" = 'Schaefer'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Steve' AND "LastName" = 'Kerns');
    
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT gen_random_uuid(), week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Steve' AND "LastName" = 'Filipovits'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Tom' AND "LastName" = 'Haeusler');
    
    RAISE NOTICE 'Inserted correct 16 Week 11 matchups';
    
END $$;

-- Verify the final result
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

-- Count total matchups
SELECT COUNT(*) as total_week11_matchups FROM "Matchups" m 
JOIN "Weeks" w ON m."WeekId" = w."Id" 
WHERE w."WeekNumber" = 11;
