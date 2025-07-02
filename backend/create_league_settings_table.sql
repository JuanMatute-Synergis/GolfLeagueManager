-- Create LeagueSettings table manually
CREATE TABLE IF NOT EXISTS "LeagueSettings" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "SeasonId" uuid NOT NULL,
    "HandicapMethod" integer NOT NULL,
    "CoursePar" integer NOT NULL,
    "CourseRating" numeric NOT NULL,
    "SlopeRating" numeric NOT NULL,
    "MaxRoundsForHandicap" integer NOT NULL,
    "ScoringMethod" integer NOT NULL,
    "PointsSystem" integer NOT NULL,
    "HoleWinPoints" integer NOT NULL,
    "HoleHalvePoints" integer NOT NULL,
    "MatchWinBonus" integer NOT NULL,
    "MatchTiePoints" integer NOT NULL,
    "UseSessionHandicaps" boolean NOT NULL,
    "AllowHandicapUpdates" boolean NOT NULL,
    "CustomRules" character varying(2000),
    "CreatedDate" timestamp with time zone NOT NULL,
    "ModifiedDate" timestamp with time zone,
    CONSTRAINT "PK_LeagueSettings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_LeagueSettings_Seasons_SeasonId" FOREIGN KEY ("SeasonId") 
        REFERENCES "Seasons" ("Id") ON DELETE CASCADE
);

-- Create index
CREATE INDEX IF NOT EXISTS "IX_LeagueSettings_SeasonId" ON "LeagueSettings" ("SeasonId");

-- Create unique constraint
CREATE UNIQUE INDEX IF NOT EXISTS "IX_LeagueSettings_SeasonId_Unique" ON "LeagueSettings" ("SeasonId");
