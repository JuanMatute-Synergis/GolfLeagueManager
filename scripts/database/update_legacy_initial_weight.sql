-- Update LegacyInitialWeight from 4 to 1 for all existing league settings
-- This corrects the default value based on user clarification

UPDATE "LeagueSettings" 
SET "LegacyInitialWeight" = 1 
WHERE "LegacyInitialWeight" = 4;

-- Verify the update
SELECT 
    "Id",
    "TenantId", 
    "AverageMethod",
    "LegacyInitialWeight"
FROM "LeagueSettings"
ORDER BY "TenantId";
