using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveJsonColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FieldsConfig",
                schema: "content",
                table: "ContentTypes");

            migrationBuilder.DropColumn(
                name: "Values",
                schema: "content",
                table: "ContentItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<JsonDocument>(
                name: "FieldsConfig",
                schema: "content",
                table: "ContentTypes",
                type: "jsonb",
                nullable: false);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "Values",
                schema: "content",
                table: "ContentItems",
                type: "jsonb",
                nullable: false);
        }
    }
}

