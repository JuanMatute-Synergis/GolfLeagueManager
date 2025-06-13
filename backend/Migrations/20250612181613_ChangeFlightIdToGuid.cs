using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    public partial class ChangeFlightIdToGuid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add new UUID column
            migrationBuilder.Sql("ALTER TABLE \"Flights\" ADD COLUMN \"Id_Guid\" uuid DEFAULT gen_random_uuid();");

            // 2. Create mapping table and assign UUIDs
            migrationBuilder.Sql("CREATE TABLE flight_id_map (old_id int PRIMARY KEY, new_id uuid NOT NULL);");
            migrationBuilder.Sql("INSERT INTO flight_id_map (old_id, new_id) SELECT \"Id\", gen_random_uuid() FROM \"Flights\";");
            migrationBuilder.Sql("UPDATE \"Flights\" SET \"Id_Guid\" = (SELECT new_id FROM flight_id_map WHERE old_id = \"Flights\".\"Id\");");

            // 3. Update all foreign keys in PlayerFlightAssignments to use the new UUIDs
            migrationBuilder.Sql("ALTER TABLE \"PlayerFlightAssignments\" ADD COLUMN \"FlightId_Guid\" uuid;");
            migrationBuilder.Sql("UPDATE \"PlayerFlightAssignments\" SET \"FlightId_Guid\" = (SELECT new_id FROM flight_id_map WHERE old_id = \"PlayerFlightAssignments\".\"FlightId\");");

            // 4. Drop old foreign key, column, and rename new column
            migrationBuilder.Sql("ALTER TABLE \"PlayerFlightAssignments\" DROP CONSTRAINT IF EXISTS \"FK_PlayerFlightAssignments_Flights_FlightId\";");
            migrationBuilder.Sql("ALTER TABLE \"PlayerFlightAssignments\" DROP COLUMN \"FlightId\";");
            migrationBuilder.Sql("ALTER TABLE \"PlayerFlightAssignments\" RENAME COLUMN \"FlightId_Guid\" TO \"FlightId\";");

            // 5. Drop old PK, column, and rename new column in Flights
            migrationBuilder.Sql("ALTER TABLE \"Flights\" DROP CONSTRAINT IF EXISTS \"PK_Flights\";");
            migrationBuilder.Sql("ALTER TABLE \"Flights\" DROP COLUMN \"Id\";");
            migrationBuilder.Sql("ALTER TABLE \"Flights\" RENAME COLUMN \"Id_Guid\" TO \"Id\";");
            migrationBuilder.Sql("ALTER TABLE \"Flights\" ADD PRIMARY KEY (\"Id\");");

            // 6. Clean up mapping table
            migrationBuilder.Sql("DROP TABLE flight_id_map;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Down migration not supported for ChangeFlightIdToGuid");
        }
    }
}
