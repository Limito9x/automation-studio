using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.DynamicForms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PragmaticEntityCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "dynamicforms",
                table: "SchemaVersions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "dynamicforms",
                table: "SchemaDefinitions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "dynamicforms",
                table: "SchemaData");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "dynamicforms",
                table: "SchemaVersions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "dynamicforms",
                table: "SchemaDefinitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "dynamicforms",
                table: "SchemaData",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
