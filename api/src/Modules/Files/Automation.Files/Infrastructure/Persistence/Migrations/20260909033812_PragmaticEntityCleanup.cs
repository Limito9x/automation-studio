using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Files.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PragmaticEntityCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "files",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "files",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "files",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "files",
                table: "AssetLinks");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "files",
                table: "AssetLinks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "files",
                table: "AssetLinks");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "files",
                table: "Assets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "files",
                table: "Assets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "files",
                table: "Assets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "files",
                table: "AssetLinks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "files",
                table: "AssetLinks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "files",
                table: "AssetLinks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
