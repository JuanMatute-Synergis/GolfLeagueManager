-- Query to check current Week 11 matchups before updating
-- Run this first to see what needs to be changed

DECLARE @WeekId UNIQUEIDENTIFIER;
DECLARE @SeasonId UNIQUEIDENTIFIER;

-- Get the current season ID
SELECT @SeasonId = Id FROM Seasons WHERE Name LIKE '%2025%' OR IsCurrent = 1;

-- Get Week 11 ID
SELECT @WeekId = Id FROM Weeks WHERE WeekNumber = 11 AND SeasonId = @SeasonId;

PRINT 'Current Week 11 Matchups:';
PRINT '=========================';

SELECT 
    CONCAT(pa.FirstName, ' ', pa.LastName) AS PlayerA,
    ' vs ',
    CONCAT(pb.FirstName, ' ', pb.LastName) AS PlayerB,
    f.Name AS Flight
FROM Matchups m
JOIN Weeks w ON m.WeekId = w.Id
JOIN Players pa ON m.PlayerAId = pa.Id
JOIN Players pb ON m.PlayerBId = pb.Id
LEFT JOIN FlightAssignments fa ON fa.PlayerId = pa.Id 
LEFT JOIN Flights f ON fa.FlightId = f.Id AND f.SeasonId = w.SeasonId
WHERE w.WeekNumber = 11 AND w.SeasonId = @SeasonId
ORDER BY f.Name, pa.LastName;

PRINT '';
PRINT 'Players in database (for reference):';
PRINT '====================================';

SELECT 
    CONCAT(FirstName, ' ', LastName) AS PlayerName,
    f.Name AS Flight
FROM Players p
LEFT JOIN FlightAssignments fa ON fa.PlayerId = p.Id
LEFT JOIN Flights f ON fa.FlightId = f.Id AND f.SeasonId = @SeasonId
WHERE EXISTS (SELECT 1 FROM FlightAssignments fa2 JOIN Flights f2 ON fa2.FlightId = f2.Id WHERE fa2.PlayerId = p.Id AND f2.SeasonId = @SeasonId)
ORDER BY f.Name, p.LastName;
