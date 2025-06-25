-- Update weeks to alternate between Front (0) and Back (1) nine holes
-- Week 1, 3, 5, 7, 9, etc. = Front 9 (0)
-- Week 2, 4, 6, 8, 10, etc. = Back 9 (1)

UPDATE "Weeks" 
SET "NineHoles" = CASE 
    WHEN "WeekNumber" % 2 = 1 THEN 0  -- Odd weeks = Front 9
    ELSE 1                            -- Even weeks = Back 9
END;

-- Verify the update
SELECT "WeekNumber", "Name", "NineHoles",
       CASE 
           WHEN "NineHoles" = 0 THEN 'Front 9'
           WHEN "NineHoles" = 1 THEN 'Back 9'
           ELSE 'Unknown'
       END as "NineHolesDisplay"
FROM "Weeks" 
ORDER BY "WeekNumber";
