using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Workspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PragmaticEntityCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "workspace",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "workspace",
                table: "WorkspacePlatforms");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "workspace",
                table: "WorkspacePlatforms");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "workspace",
                table: "WorkspacePlatforms");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "workspace",
                table: "WorkspaceAgents");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "workspace",
                table: "WorkspaceAgents");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "workspace",
                table: "WorkspaceAgents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "workspace",
                table: "ResourceVersions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "workspace",
                table: "ResourceVersions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "workspace",
                table: "ResourceVersions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "workspace",
                table: "ResourceVersionLocations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "workspace",
                table: "ResourceVersionLocations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "workspace",
                table: "ResourceVersionLocations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "workspace",
                table: "ResourceItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "workspace",
                table: "Workspaces",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "workspace",
                table: "WorkspacePlatforms",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "workspace",
                table: "WorkspacePlatforms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "workspace",
                table: "WorkspacePlatforms",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "workspace",
                table: "WorkspaceAgents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "workspace",
                table: "WorkspaceAgents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "workspace",
                table: "WorkspaceAgents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "workspace",
                table: "ResourceVersions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "workspace",
                table: "ResourceVersions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "workspace",
                table: "ResourceVersions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "workspace",
                table: "ResourceVersionLocations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "workspace",
                table: "ResourceVersionLocations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "workspace",
                table: "ResourceVersionLocations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "workspace",
                table: "ResourceItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
