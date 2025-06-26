-- Enhanced Delete and Import Week 11 Matchups Script
-- This script handles name variations and provides better error checking

-- Step 1: Find the Week 11 ID for the current season
DECLARE @WeekId UNIQUEIDENTIFIER;
DECLARE @SeasonId UNIQUEIDENTIFIER;

-- Get the current season ID (adjust this query based on your season naming)
SELECT @SeasonId = Id FROM Seasons WHERE Name LIKE '%2025%' OR IsCurrent = 1;

-- Get Week 11 ID
SELECT @WeekId = Id FROM Weeks WHERE WeekNumber = 11 AND SeasonId = @SeasonId;

PRINT 'Season ID: ' + CAST(@SeasonId AS VARCHAR(50));
PRINT 'Week 11 ID: ' + CAST(@WeekId AS VARCHAR(50));

-- Check if we found the week
IF @WeekId IS NULL
BEGIN
    PRINT 'ERROR: Could not find Week 11 for the current season';
    RETURN;
END

-- Step 2: Delete existing matchups for Week 11
DELETE FROM HoleScores 
WHERE MatchupId IN (SELECT Id FROM Matchups WHERE WeekId = @WeekId);

DELETE FROM Matchups 
WHERE WeekId = @WeekId;

PRINT 'Deleted existing Week 11 matchups and hole scores';

-- Step 3: Helper function to find player by name with error checking
-- Let's check what players we have first
PRINT 'Checking player names in database...';

-- Step 4: Insert new matchups with better error handling
DECLARE @PlayerAId UNIQUEIDENTIFIER, @PlayerBId UNIQUEIDENTIFIER;

-- Flight 1 Matchups
-- George Hutson vs Kevin Kelhart
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'George' AND LastName = 'Hutson';
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'Kevin' AND LastName = 'Kelhart' AND Id = (SELECT MIN(Id) FROM Players WHERE FirstName = 'Kevin' AND LastName = 'Kelhart');
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find George Hutson or Kevin Kelhart';

-- Jeff Dilcher vs John Perry
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'Jeff' AND LastName = 'Dilcher';
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'John' AND LastName = 'Perry';
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find Jeff Dilcher or John Perry';

-- Bill Stein vs Alex Peck
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'Bill' AND LastName = 'Stein';
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'Alex' AND LastName = 'Peck';
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find Bill Stein or Alex Peck';

-- Tim Seyler vs Joe Mahachanh
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'Tim' AND LastName = 'Seyler';
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'Joe' AND LastName = 'Mahachanh';
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find Tim Seyler or Joe Mahachanh';

-- Flight 2 Matchups
-- Carl Hardner vs Lou Gabrielle
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'Carl' AND LastName = 'Hardner';
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'Lou' AND LastName = 'Gabrielle';
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find Carl Hardner or Lou Gabrielle';

-- Jay Sullivan vs Kenny Palladino
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'Jay' AND LastName = 'Sullivan';
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'Kenny' AND LastName = 'Palladino';
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find Jay Sullivan or Kenny Palladino';

-- Stu Silfies vs Steve Bedek
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'Stu' AND LastName = 'Silfies';
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'Steve' AND LastName = 'Bedek';
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find Stu Silfies or Steve Bedek';

-- Curt Saeger vs Frank Frankenfield
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'Curt' AND LastName = 'Saeger';
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'Frank' AND LastName = 'Frankenfield';
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find Curt Saeger or Frank Frankenfield';

-- Flight 3 Matchups
-- Matt Speth vs Juan Matute
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'Matt' AND LastName = 'Speth';
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'Juan' AND LastName = 'Matute';
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find Matt Speth or Juan Matute';

-- Jim Eck vs Danny Washurn
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'Jim' AND LastName = 'Eck';
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'Danny' AND LastName = 'Washurn';
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find Jim Eck or Danny Washurn';

-- Kevin Kelhart JR vs Steve Hampton (need to get the other Kevin Kelhart)
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'Kevin' AND LastName = 'Kelhart' AND Id != (SELECT MIN(Id) FROM Players WHERE FirstName = 'Kevin' AND LastName = 'Kelhart');
IF @PlayerAId IS NULL
    SELECT @PlayerAId = Id FROM Players WHERE (FirstName LIKE 'Kevin%' AND LastName = 'Kelhart') OR (FirstName = 'Kevin' AND LastName LIKE 'Kelhart%');
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'Steve' AND LastName = 'Hampton';
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find Kevin Kelhart JR or Steve Hampton';

-- Bob Gross vs Matt Donahue
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'Bob' AND LastName = 'Gross';
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'Matt' AND LastName = 'Donahue';
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find Bob Gross or Matt Donahue';

-- Flight 4 Matchups
-- Ray Ballinger vs Andrew Kerns
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'Ray' AND LastName = 'Ballinger';
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'Andrew' AND LastName = 'Kerns';
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find Ray Ballinger or Andrew Kerns';

-- Rich Hart vs Jax Haeusler
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'Rich' AND LastName = 'Hart';
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'Jax' AND LastName = 'Haeusler';
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find Rich Hart or Jax Haeusler';

-- Mike Schaefer vs Steve Kerns
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'Mike' AND LastName = 'Schaefer';
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'Steve' AND LastName = 'Kerns';
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find Mike Schaefer or Steve Kerns';

-- Steve Filipovits vs Tom Haeusler
SELECT @PlayerAId = Id FROM Players WHERE FirstName = 'Steve' AND LastName = 'Filipovits';
SELECT @PlayerBId = Id FROM Players WHERE FirstName = 'Tom' AND LastName = 'Haeusler';
IF @PlayerAId IS NOT NULL AND @PlayerBId IS NOT NULL
    INSERT INTO Matchups (Id, WeekId, PlayerAId, PlayerBId) VALUES (NEWID(), @WeekId, @PlayerAId, @PlayerBId);
ELSE
    PRINT 'ERROR: Could not find Steve Filipovits or Tom Haeusler';

PRINT 'Finished inserting Week 11 matchups';

-- Step 5: Verify the import
PRINT 'Verifying Week 11 matchups:';
SELECT 
    CONCAT(pa.FirstName, ' ', pa.LastName) AS PlayerA,
    CONCAT(pb.FirstName, ' ', pb.LastName) AS PlayerB
FROM Matchups m
JOIN Weeks w ON m.WeekId = w.Id
JOIN Players pa ON m.PlayerAId = pa.Id
JOIN Players pb ON m.PlayerBId = pb.Id
WHERE w.WeekNumber = 11 AND w.SeasonId = @SeasonId
ORDER BY pa.LastName;

PRINT 'Week 11 matchups update completed!';
