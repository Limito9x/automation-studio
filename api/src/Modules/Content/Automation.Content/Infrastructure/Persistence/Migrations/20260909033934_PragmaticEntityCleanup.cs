using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PragmaticEntityCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContentItems_ProjectId",
                schema: "content",
                table: "ContentItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "content",
                table: "ContentTypes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "content",
                table: "ContentItems");

            

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_ProjectId_ContentTypeId_Name",
                schema: "content",
                table: "ContentItems",
                columns: new[] { "ProjectId", "ContentTypeId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContentItems_ProjectId_ContentTypeId_Name",
                schema: "content",
                table: "ContentItems");

            

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "content",
                table: "ContentTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "content",
                table: "ContentItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_ProjectId",
                schema: "content",
                table: "ContentItems",
                column: "ProjectId");
        }
    }
}
