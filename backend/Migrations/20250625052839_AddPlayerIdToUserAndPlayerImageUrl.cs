using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerIdToUserAndPlayerImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlayerId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Players",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PlayerId",
                table: "Users",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Players_PlayerId",
                table: "Users",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Players_PlayerId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PlayerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PlayerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Players");
        }
    }
}
