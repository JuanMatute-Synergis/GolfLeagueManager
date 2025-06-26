-- Delete and Import Week 11 Matchups Script
-- This script deletes existing Week 11 matchups and imports the correct ones

-- Step 1: Find the Week 11 ID for the current season
-- You'll need to replace 'SEASON_ID_HERE' with the actual season ID

DECLARE @WeekId UNIQUEIDENTIFIER;
DECLARE @SeasonId UNIQUEIDENTIFIER;

-- Get the current season ID (adjust this query based on your season naming)
SELECT @SeasonId = Id FROM Seasons WHERE Name LIKE '%2025%' OR IsCurrent = 1;

-- Get Week 11 ID
SELECT @WeekId = Id FROM Weeks WHERE WeekNumber = 11 AND SeasonId = @SeasonId;

PRINT 'Season ID: ' + CAST(@SeasonId AS VARCHAR(50));
PRINT 'Week 11 ID: ' + CAST(@WeekId AS VARCHAR(50));

-- Step 2: Delete existing matchups for Week 11
DELETE FROM HoleScores 
WHERE MatchupId IN (SELECT Id FROM Matchups WHERE WeekId = @WeekId);

DELETE FROM Matchups 
WHERE WeekId = @WeekId;

PRINT 'Deleted existing Week 11 matchups and hole scores';

-- Step 3: Insert new matchups based on the CSV data
-- We need to get player IDs and flight IDs first

-- Flight A (Flight 1)
INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'George' AND LastName = 'Hutson'),
    (SELECT Id FROM Players WHERE FirstName = 'Kevin' AND LastName = 'Kelhart');

INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'Jeff' AND LastName = 'Dilcher'),
    (SELECT Id FROM Players WHERE FirstName = 'John' AND LastName = 'Perry');

INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'Bill' AND LastName = 'Stein'),
    (SELECT Id FROM Players WHERE FirstName = 'Alex' AND LastName = 'Peck');

INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'Tim' AND LastName = 'Seyler'),
    (SELECT Id FROM Players WHERE FirstName = 'Joe' AND LastName = 'Mahachanh');

-- Flight B (Flight 2)
INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'Carl' AND LastName = 'Hardner'),
    (SELECT Id FROM Players WHERE FirstName = 'Lou' AND LastName = 'Gabrielle');

INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'Jay' AND LastName = 'Sullivan'),
    (SELECT Id FROM Players WHERE FirstName = 'Kenny' AND LastName = 'Palladino');

INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'Stu' AND LastName = 'Silfies'),
    (SELECT Id FROM Players WHERE FirstName = 'Steve' AND LastName = 'Bedek');

INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'Curt' AND LastName = 'Saeger'),
    (SELECT Id FROM Players WHERE FirstName = 'Frank' AND LastName = 'Frankenfield');

-- Flight C (Flight 3)
INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'Matt' AND LastName = 'Speth'),
    (SELECT Id FROM Players WHERE FirstName = 'Juan' AND LastName = 'Matute');

INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'Jim' AND LastName = 'Eck'),
    (SELECT Id FROM Players WHERE FirstName = 'Danny' AND LastName = 'Washurn');

INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'Kevin' AND LastName = 'Kelhart' AND Id != (SELECT Id FROM Players WHERE FirstName = 'Kevin' AND LastName = 'Kelhart' ORDER BY Id OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY)),
    (SELECT Id FROM Players WHERE FirstName = 'Steve' AND LastName = 'Hampton');

INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'Bob' AND LastName = 'Gross'),
    (SELECT Id FROM Players WHERE FirstName = 'Matt' AND LastName = 'Donahue');

-- Flight D (Flight 4)
INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'Ray' AND LastName = 'Ballinger'),
    (SELECT Id FROM Players WHERE FirstName = 'Andrew' AND LastName = 'Kerns');

INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'Rich' AND LastName = 'Hart'),
    (SELECT Id FROM Players WHERE FirstName = 'Jax' AND LastName = 'Haeusler');

INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'Mike' AND LastName = 'Schaefer'),
    (SELECT Id FROM Players WHERE FirstName = 'Steve' AND LastName = 'Kerns');

INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId)
SELECT 
    NEWID(),
    @WeekId,
    (SELECT Id FROM Players WHERE FirstName = 'Steve' AND LastName = 'Filipovits'),
    (SELECT Id FROM Players WHERE FirstName = 'Tom' AND LastName = 'Haeusler');

PRINT 'Inserted new Week 11 matchups';

-- Step 4: Verify the import
SELECT 
    w.WeekNumber,
    CONCAT(pa.FirstName, ' ', pa.LastName) AS PlayerA,
    CONCAT(pb.FirstName, ' ', pb.LastName) AS PlayerB,
    f.Name AS Flight
FROM Matchups m
JOIN Weeks w ON m.WeekId = w.Id
JOIN Players pa ON m.PlayerAId = pa.Id
JOIN Players pb ON m.PlayerBId = pb.Id
LEFT JOIN FlightAssignments fa ON fa.PlayerId = pa.Id AND fa.Flight.SeasonId = w.SeasonId
LEFT JOIN Flights f ON fa.FlightId = f.Id
WHERE w.WeekNumber = 11 AND w.SeasonId = @SeasonId
ORDER BY f.Name, pa.LastName;

PRINT 'Week 11 matchups updated successfully!';
