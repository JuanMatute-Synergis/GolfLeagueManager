-- Update PlayerSeasonRecords InitialAverageScore to match reference data
-- Using exact name matching with FirstName and LastName

-- George Hutson
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 41.17
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'George' AND "Players"."LastName" = 'Hutson';

-- Jeff Dilcher  
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 41.43
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Jeff' AND "Players"."LastName" = 'Dilcher';

-- Bill Stein
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 42.30
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Bill' AND "Players"."LastName" = 'Stein';

-- Alex Peck
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 42.75
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Alex' AND "Players"."LastName" = 'Peck';

-- Tim Seyler
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 43.22
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Tim' AND "Players"."LastName" = 'Seyler';

-- Kevin Kelhart (main one)
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 43.52
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Kevin' AND "Players"."LastName" = 'Kelhart'
AND "PlayerSeasonRecords"."InitialAverageScore" = 43.52;

-- Joe Mahachanh
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 43.57
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Joe' AND "Players"."LastName" = 'Mahachanh';

-- John Perry
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 43.60
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'John' AND "Players"."LastName" = 'Perry';

-- Carl Hardner
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 43.60
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Carl' AND "Players"."LastName" = 'Hardner';

-- Jay Sullivan
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 44.10
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Jay' AND "Players"."LastName" = 'Sullivan';

-- Stu Silfies
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 44.63
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Stu' AND "Players"."LastName" = 'Silfies';

-- Steve Bedek
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 44.68
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Steve' AND "Players"."LastName" = 'Bedek';

-- Curt Saeger
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 44.79
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Curt' AND "Players"."LastName" = 'Saeger';

-- Lou Gabrielle
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 44.85
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Lou' AND "Players"."LastName" = 'Gabrielle';

-- Frank Frankenfield
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 44.97
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Frank' AND "Players"."LastName" = 'Frankenfield';

-- Kenny Palladino
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 44.99
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Kenny' AND "Players"."LastName" = 'Palladino';

-- Matt Speth
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 45.24
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Matt' AND "Players"."LastName" = 'Speth';

-- Jim Eck
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 46.45
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Jim' AND "Players"."LastName" = 'Eck';

-- Kevin Kelhart JR (the one with 46.58 average - update this specific record)
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 46.58
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Kevin' AND "Players"."LastName" = 'Kelhart JR'
AND "PlayerSeasonRecords"."InitialAverageScore" = 46.58;

-- Steve Hampton
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 47.59
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Steve' AND "Players"."LastName" = 'Hampton';

-- Bob Gross
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 47.85
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Bob' AND "Players"."LastName" = 'Gross';

-- Juan Matute, Matt Donahue, Danny Washurn (all 47.95)
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 47.95
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND (("Players"."FirstName" = 'Juan' AND "Players"."LastName" = 'Matute') OR
     ("Players"."FirstName" = 'Matt' AND "Players"."LastName" = 'Donahue') OR
     ("Players"."FirstName" = 'Danny' AND "Players"."LastName" = 'Washurn'));

-- Ray Ballinger
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 48.42
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Ray' AND "Players"."LastName" = 'Ballinger'
AND "PlayerSeasonRecords"."InitialAverageScore" != 45;

-- Rich Hart
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 50.71
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Rich' AND "Players"."LastName" = 'Hart';

-- Mike Schaefer
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 51.43
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Mike' AND "Players"."LastName" = 'Schaefer';

-- Steve Kerns
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 53.32
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Steve' AND "Players"."LastName" = 'Kerns';

-- Steve Filipovits
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 55.20
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Steve' AND "Players"."LastName" = 'Filipovits';

-- Andrew Kerns
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 55.37
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND "Players"."FirstName" = 'Andrew' AND "Players"."LastName" = 'Kerns';

-- Tom Haeusler and Jax Haeusler (both 60.50)
UPDATE "PlayerSeasonRecords" 
SET "InitialAverageScore" = 60.50
FROM "Players" 
WHERE "PlayerSeasonRecords"."PlayerId" = "Players"."Id" 
AND (("Players"."FirstName" = 'Tom' AND "Players"."LastName" = 'Haeusler') OR
     ("Players"."FirstName" = 'Jax' AND "Players"."LastName" = 'Haeusler'));
