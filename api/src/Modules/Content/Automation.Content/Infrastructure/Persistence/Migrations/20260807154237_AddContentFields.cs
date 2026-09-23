using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                schema: "content",
                table: "ContentTypes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "content",
                table: "ContentTypes",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                schema: "content",
                table: "ContentTypes",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                schema: "content",
                table: "ContentTypes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "content",
                table: "ContentTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "content",
                table: "ContentItems",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                schema: "content",
                table: "ContentTypes");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "content",
                table: "ContentTypes");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                schema: "content",
                table: "ContentTypes");

            migrationBuilder.DropColumn(
                name: "Icon",
                schema: "content",
                table: "ContentTypes");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "content",
                table: "ContentTypes");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "content",
                table: "ContentItems");
        }
    }
}

