using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangePlayerIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add a new UUID column
            migrationBuilder.AddColumn<Guid>(
                name: "NewId",
                table: "Players",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            // Step 2: Generate UUID values for existing records
            migrationBuilder.Sql("UPDATE \"Players\" SET \"NewId\" = gen_random_uuid()");

            // Step 3: Drop the old Id column
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Players");

            // Step 4: Rename NewId to Id
            migrationBuilder.RenameColumn(
                name: "NewId",
                table: "Players",
                newName: "Id");

            // Step 5: Add primary key constraint
            migrationBuilder.AddPrimaryKey(
                name: "PK_Players",
                table: "Players",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop the primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_Players",
                table: "Players");

            // Step 2: Add a new integer column
            migrationBuilder.AddColumn<int>(
                name: "NewId",
                table: "Players",
                type: "integer",
                nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // Step 3: Drop the UUID Id column
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Players");

            // Step 4: Rename NewId to Id
            migrationBuilder.RenameColumn(
                name: "NewId",
                table: "Players",
                newName: "Id");

            // Step 5: Add primary key constraint
            migrationBuilder.AddPrimaryKey(
                name: "PK_Players",
                table: "Players",
                column: "Id");
        }
    }
}
