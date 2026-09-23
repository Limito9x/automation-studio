using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Files.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorAssetArchitecture_SHA256 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assets_HardHash",
                schema: "files",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "HardHash",
                schema: "files",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "OriginalName",
                schema: "files",
                table: "Assets");

            migrationBuilder.AddColumn<string>(
                name: "HashSha256",
                schema: "files",
                table: "Assets",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginalName",
                schema: "files",
                table: "AssetLinks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_HashSha256",
                schema: "files",
                table: "Assets",
                column: "HashSha256");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assets_HashSha256",
                schema: "files",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "HashSha256",
                schema: "files",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "OriginalName",
                schema: "files",
                table: "AssetLinks");

            migrationBuilder.AddColumn<string>(
                name: "HardHash",
                schema: "files",
                table: "Assets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginalName",
                schema: "files",
                table: "Assets",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_HardHash",
                schema: "files",
                table: "Assets",
                column: "HardHash");
        }
    }
}



