using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Files.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHashSha256Unique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assets_HashSha256",
                schema: "files",
                table: "Assets");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_HashSha256",
                schema: "files",
                table: "Assets",
                column: "HashSha256",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assets_HashSha256",
                schema: "files",
                table: "Assets");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_HashSha256",
                schema: "files",
                table: "Assets",
                column: "HashSha256");
        }
    }
}



