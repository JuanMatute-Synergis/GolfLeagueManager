CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE TABLE "Players" (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "FirstName" text NOT NULL,
        "LastName" text NOT NULL,
        "Email" text NOT NULL,
        "Phone" text NOT NULL,
        CONSTRAINT "PK_Players" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE TABLE "Seasons" (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "Name" character varying(100) NOT NULL,
        "Year" integer NOT NULL,
        "SeasonNumber" integer NOT NULL,
        "StartDate" timestamp with time zone NOT NULL,
        "EndDate" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Seasons" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE TABLE "Flights" (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "Name" text NOT NULL,
        "MaxPlayers" integer NOT NULL,
        "Description" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "SeasonId" uuid,
        CONSTRAINT "PK_Flights" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Flights_Seasons_SeasonId" FOREIGN KEY ("SeasonId") REFERENCES "Seasons" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE TABLE "Weeks" (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "WeekNumber" integer NOT NULL,
        "StartDate" timestamp with time zone NOT NULL,
        "EndDate" timestamp with time zone NOT NULL,
        "Name" character varying(100) NOT NULL,
        "IsActive" boolean NOT NULL,
        "SeasonId" uuid NOT NULL,
        CONSTRAINT "PK_Weeks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Weeks_Seasons_SeasonId" FOREIGN KEY ("SeasonId") REFERENCES "Seasons" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE TABLE "PlayerFlightAssignments" (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "PlayerId" uuid NOT NULL,
        "FlightId" uuid NOT NULL,
        "IsFlightLeader" boolean NOT NULL,
        "HandicapAtAssignment" double precision NOT NULL,
        CONSTRAINT "PK_PlayerFlightAssignments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PlayerFlightAssignments_Flights_FlightId" FOREIGN KEY ("FlightId") REFERENCES "Flights" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_PlayerFlightAssignments_Players_PlayerId" FOREIGN KEY ("PlayerId") REFERENCES "Players" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE TABLE "Matchups" (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "WeekId" uuid NOT NULL,
        "PlayerAId" uuid NOT NULL,
        "PlayerBId" uuid NOT NULL,
        "PlayerAScore" integer,
        "PlayerBScore" integer,
        "PlayerAPoints" integer,
        "PlayerBPoints" integer,
        CONSTRAINT "PK_Matchups" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Matchups_Players_PlayerAId" FOREIGN KEY ("PlayerAId") REFERENCES "Players" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Matchups_Players_PlayerBId" FOREIGN KEY ("PlayerBId") REFERENCES "Players" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Matchups_Weeks_WeekId" FOREIGN KEY ("WeekId") REFERENCES "Weeks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE TABLE "ScoreEntries" (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "PlayerId" uuid NOT NULL,
        "WeekId" uuid NOT NULL,
        "Score" integer NOT NULL,
        "PointsEarned" integer NOT NULL,
        CONSTRAINT "PK_ScoreEntries" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ScoreEntries_Players_PlayerId" FOREIGN KEY ("PlayerId") REFERENCES "Players" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ScoreEntries_Weeks_WeekId" FOREIGN KEY ("WeekId") REFERENCES "Weeks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE INDEX "IX_Flights_SeasonId" ON "Flights" ("SeasonId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE INDEX "IX_Matchups_PlayerAId" ON "Matchups" ("PlayerAId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE INDEX "IX_Matchups_PlayerBId" ON "Matchups" ("PlayerBId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE INDEX "IX_Matchups_WeekId" ON "Matchups" ("WeekId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE INDEX "IX_PlayerFlightAssignments_FlightId" ON "PlayerFlightAssignments" ("FlightId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE INDEX "IX_PlayerFlightAssignments_PlayerId" ON "PlayerFlightAssignments" ("PlayerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE INDEX "IX_ScoreEntries_PlayerId" ON "ScoreEntries" ("PlayerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE INDEX "IX_ScoreEntries_WeekId" ON "ScoreEntries" ("WeekId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    CREATE INDEX "IX_Weeks_SeasonId" ON "Weeks" ("SeasonId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250614130308_InitialCreateWithGuids') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20250614130308_InitialCreateWithGuids', '9.0.5');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250617001021_AddHoleScoreTable') THEN
    CREATE TABLE "HoleScores" (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "MatchupId" uuid NOT NULL,
        "HoleNumber" integer NOT NULL,
        "Par" integer NOT NULL,
        "PlayerAScore" integer,
        "PlayerBScore" integer,
        CONSTRAINT "PK_HoleScores" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_HoleScores_Matchups_MatchupId" FOREIGN KEY ("MatchupId") REFERENCES "Matchups" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250617001021_AddHoleScoreTable') THEN
    CREATE UNIQUE INDEX "IX_HoleScores_MatchupId_HoleNumber" ON "HoleScores" ("MatchupId", "HoleNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250617001021_AddHoleScoreTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20250617001021_AddHoleScoreTable', '9.0.5');
    END IF;
END $EF$;
COMMIT;

