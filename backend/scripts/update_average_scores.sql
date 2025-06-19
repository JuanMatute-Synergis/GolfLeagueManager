-- Update player average scores based on CSV data
-- Script to populate both CurrentAverageScore and InitialAverageScore for all players

UPDATE "Players" SET "CurrentAverageScore" = 41.24, "InitialAverageScore" = 41.24 WHERE "FirstName" = 'George' AND "LastName" = 'Hutson';
UPDATE "Players" SET "CurrentAverageScore" = 41.96, "InitialAverageScore" = 41.96 WHERE "FirstName" = 'Jeff' AND "LastName" = 'Dilcher';
UPDATE "Players" SET "CurrentAverageScore" = 42.32, "InitialAverageScore" = 42.32 WHERE "FirstName" = 'Bill' AND "LastName" = 'Stein';
UPDATE "Players" SET "CurrentAverageScore" = 42.86, "InitialAverageScore" = 42.86 WHERE "FirstName" = 'Alex' AND "LastName" = 'Peck';
UPDATE "Players" SET "CurrentAverageScore" = 43.98, "InitialAverageScore" = 43.98 WHERE "FirstName" = 'Tim' AND "LastName" = 'Seyler';
UPDATE "Players" SET "CurrentAverageScore" = 43.30, "InitialAverageScore" = 43.30 WHERE "FirstName" = 'Kevin' AND "LastName" = 'Kelhart';
UPDATE "Players" SET "CurrentAverageScore" = 43.61, "InitialAverageScore" = 43.61 WHERE "FirstName" = 'Joe' AND "LastName" = 'Mahachanh';
UPDATE "Players" SET "CurrentAverageScore" = 42.63, "InitialAverageScore" = 42.63 WHERE "FirstName" = 'John' AND "LastName" = 'Perry';
UPDATE "Players" SET "CurrentAverageScore" = 43.73, "InitialAverageScore" = 43.73 WHERE "FirstName" = 'Carl' AND "LastName" = 'Hardner';
UPDATE "Players" SET "CurrentAverageScore" = 44.10, "InitialAverageScore" = 44.10 WHERE "FirstName" = 'Jay' AND "LastName" = 'Sullivan';
UPDATE "Players" SET "CurrentAverageScore" = 44.75, "InitialAverageScore" = 44.75 WHERE "FirstName" = 'Stu' AND "LastName" = 'Silfies';
UPDATE "Players" SET "CurrentAverageScore" = 45.53, "InitialAverageScore" = 45.53 WHERE "FirstName" = 'Steve' AND "LastName" = 'Bedek';
UPDATE "Players" SET "CurrentAverageScore" = 43.88, "InitialAverageScore" = 43.88 WHERE "FirstName" = 'Curt' AND "LastName" = 'Saeger';
UPDATE "Players" SET "CurrentAverageScore" = 45.91, "InitialAverageScore" = 45.91 WHERE "FirstName" = 'Lou' AND "LastName" = 'Gabrielle';
UPDATE "Players" SET "CurrentAverageScore" = 45.55, "InitialAverageScore" = 45.55 WHERE "FirstName" = 'Frank' AND "LastName" = 'Frankenfield';
UPDATE "Players" SET "CurrentAverageScore" = 44.33, "InitialAverageScore" = 44.33 WHERE "FirstName" = 'Kenny' AND "LastName" = 'Palladino';
UPDATE "Players" SET "CurrentAverageScore" = 45.85, "InitialAverageScore" = 45.85 WHERE "FirstName" = 'Matt' AND "LastName" = 'Speth';
UPDATE "Players" SET "CurrentAverageScore" = 46.80, "InitialAverageScore" = 46.80 WHERE "FirstName" = 'Jim' AND "LastName" = 'Eck';
UPDATE "Players" SET "CurrentAverageScore" = 48.22, "InitialAverageScore" = 48.22 WHERE "FirstName" = 'Kevin' AND "LastName" = 'Kelhart JR';
UPDATE "Players" SET "CurrentAverageScore" = 47.62, "InitialAverageScore" = 47.62 WHERE "FirstName" = 'Steve' AND "LastName" = 'Hampton';
UPDATE "Players" SET "CurrentAverageScore" = 47.57, "InitialAverageScore" = 47.57 WHERE "FirstName" = 'Bob' AND "LastName" = 'Gross';
UPDATE "Players" SET "CurrentAverageScore" = 47.69, "InitialAverageScore" = 47.69 WHERE "FirstName" = 'Juan' AND "LastName" = 'Matute';
UPDATE "Players" SET "CurrentAverageScore" = 48.97, "InitialAverageScore" = 48.97 WHERE "FirstName" = 'Matt' AND "LastName" = 'Donahue';
UPDATE "Players" SET "CurrentAverageScore" = 48.69, "InitialAverageScore" = 48.69 WHERE "FirstName" = 'Danny' AND "LastName" = 'Washburn';
UPDATE "Players" SET "CurrentAverageScore" = 46.96, "InitialAverageScore" = 46.96 WHERE "FirstName" = 'Ray' AND "LastName" = 'Ballinger';
UPDATE "Players" SET "CurrentAverageScore" = 50.83, "InitialAverageScore" = 50.83 WHERE "FirstName" = 'Rich' AND "LastName" = 'Hart';
UPDATE "Players" SET "CurrentAverageScore" = 50.53, "InitialAverageScore" = 50.53 WHERE "FirstName" = 'Mike' AND "LastName" = 'Schaefer';
UPDATE "Players" SET "CurrentAverageScore" = 53.18, "InitialAverageScore" = 53.18 WHERE "FirstName" = 'Steve' AND "LastName" = 'Kerns';
UPDATE "Players" SET "CurrentAverageScore" = 55.40, "InitialAverageScore" = 55.40 WHERE "FirstName" = 'Steve' AND "LastName" = 'Filipovits';
UPDATE "Players" SET "CurrentAverageScore" = 54.21, "InitialAverageScore" = 54.21 WHERE "FirstName" = 'Andrew' AND "LastName" = 'Kerns';
UPDATE "Players" SET "CurrentAverageScore" = 58.50, "InitialAverageScore" = 58.50 WHERE "FirstName" = 'Tom' AND "LastName" = 'Haeusler';
UPDATE "Players" SET "CurrentAverageScore" = 57.17, "InitialAverageScore" = 57.17 WHERE "FirstName" = 'Jax' AND "LastName" = 'Haeusler';

-- Check for players that might not have been updated (name mismatches)
SELECT "FirstName" || ' ' || "LastName" as "FullName", "CurrentAverageScore", "InitialAverageScore"
FROM "Players" 
WHERE ("CurrentAverageScore" = 0.0 OR "CurrentAverageScore" IS NULL) 
   OR ("InitialAverageScore" = 0.0 OR "InitialAverageScore" IS NULL)
ORDER BY "LastName", "FirstName";
