-- Delete and Import Week 11 Matchups Script for PostgreSQL (Corrected)
-- This script deletes existing Week 11 matchups and imports the correct ones

DO $$
DECLARE
    week_id UUID;
    season_id UUID;
BEGIN
    -- Step 1: Find the Week 11 ID for the current season
    SELECT "Id" INTO season_id FROM "Seasons" WHERE "Year" = 2025 LIMIT 1;
    
    -- Get Week 11 ID
    SELECT "Id" INTO week_id FROM "Weeks" WHERE "WeekNumber" = 11 AND "SeasonId" = season_id;
    
    RAISE NOTICE 'Season ID: %', season_id;
    RAISE NOTICE 'Week 11 ID: %', week_id;
    
    IF week_id IS NULL THEN
        RAISE EXCEPTION 'Could not find Week 11 for the 2025 season';
    END IF;
    
    -- Step 2: Delete existing matchups for Week 11
    DELETE FROM "HoleScores" 
    WHERE "MatchupId" IN (SELECT "Id" FROM "Matchups" WHERE "WeekId" = week_id);
    
    DELETE FROM "Matchups" 
    WHERE "WeekId" = week_id;
    
    RAISE NOTICE 'Deleted existing Week 11 matchups and hole scores';
    
    -- Step 3: Insert new matchups based on the CSV data
    
    -- Flight 1
    -- George Hutson vs Kevin Kelhart
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'George' AND "LastName" = 'Hutson'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Kevin' AND "LastName" = 'Kelhart' ORDER BY "Id" LIMIT 1);
    
    -- Jeff Dilcher vs John Perry
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Jeff' AND "LastName" = 'Dilcher'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'John' AND "LastName" = 'Perry');
    
    -- Bill Stein vs Alex Peck
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Bill' AND "LastName" = 'Stein'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Alex' AND "LastName" = 'Peck');
    
    -- Tim Seyler vs Joe Mahachanh
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Tim' AND "LastName" = 'Seyler'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Joe' AND "LastName" = 'Mahachanh');
    
    -- Flight 2
    -- Carl Hardner vs Lou Gabrielle
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Carl' AND "LastName" = 'Hardner'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Lou' AND "LastName" = 'Gabrielle');
    
    -- Jay Sullivan vs Kenny Palladino
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Jay' AND "LastName" = 'Sullivan'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Kenny' AND "LastName" = 'Palladino');
    
    -- Stu Silfies vs Steve Bedek
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Stu' AND "LastName" = 'Silfies'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Steve' AND "LastName" = 'Bedek');
    
    -- Curt Saeger vs Frank Frankenfield
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Curt' AND "LastName" = 'Saeger'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Frank' AND "LastName" = 'Frankenfield');
    
    -- Flight 3
    -- Matt Speth vs Juan Matute
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Matt' AND "LastName" = 'Speth'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Juan' AND "LastName" = 'Matute');
    
    -- Jim Eck vs Danny Washurn
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Jim' AND "LastName" = 'Eck'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Danny' AND "LastName" = 'Washurn');
    
    -- Kevin Kelhart JR vs Steve Hampton (get the second Kevin Kelhart)
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Kevin' AND "LastName" = 'Kelhart' ORDER BY "Id" OFFSET 1 LIMIT 1),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Steve' AND "LastName" = 'Hampton');
    
    -- Bob Gross vs Matt Donahue
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Bob' AND "LastName" = 'Gross'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Matt' AND "LastName" = 'Donahue');
    
    -- Flight 4
    -- Ray Ballinger vs Andrew Kerns
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Ray' AND "LastName" = 'Ballinger'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Andrew' AND "LastName" = 'Kerns');
    
    -- Rich Hart vs Jax Haeusler
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Rich' AND "LastName" = 'Hart'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Jax' AND "LastName" = 'Haeusler');
    
    -- Mike Schaefer vs Steve Kerns
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Mike' AND "LastName" = 'Schaefer'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Steve' AND "LastName" = 'Kerns');
    
    -- Steve Filipovits vs Tom Haeusler
    INSERT INTO "Matchups" ("Id", "WeekId", "PlayerAId", "PlayerBId")
    SELECT 
        gen_random_uuid(),
        week_id,
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Steve' AND "LastName" = 'Filipovits'),
        (SELECT "Id" FROM "Players" WHERE "FirstName" = 'Tom' AND "LastName" = 'Haeusler');
    
    RAISE NOTICE 'Inserted new Week 11 matchups';
    
END $$;

-- Step 4: Verify the import
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
