using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PragmaticEntityCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "platform",
                table: "Platforms");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "platform",
                table: "PlatformExtensions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "platform",
                table: "PlatformExtensions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "platform",
                table: "PlatformExtensions");

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "platform",
                table: "Platforms",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "platform",
                table: "PlatformExtensions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "platform",
                table: "PlatformExtensions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "platform",
                table: "PlatformExtensions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
