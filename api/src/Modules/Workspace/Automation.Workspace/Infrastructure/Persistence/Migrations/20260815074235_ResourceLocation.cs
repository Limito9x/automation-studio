using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Workspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ResourceLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ResourceVersionLocations_WorkspaceAgentId_RelativePath",
                schema: "workspace",
                table: "ResourceVersionLocations");

            migrationBuilder.DropIndex(
                name: "IX_ResourceItems_ProjectId",
                schema: "workspace",
                table: "ResourceItems");

            migrationBuilder.DropIndex(
                name: "IX_ResourceItems_WorkspaceId",
                schema: "workspace",
                table: "ResourceItems");

            migrationBuilder.DropColumn(
                name: "RelativePath",
                schema: "workspace",
                table: "ResourceVersionLocations");

            migrationBuilder.DropColumn(
                name: "FilePath",
                schema: "workspace",
                table: "ResourceItems");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "workspace",
                table: "ResourceItems");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "workspace",
                table: "ResourceItems",
                newName: "DisplayName");

            migrationBuilder.AlterColumn<string>(
                name: "FileHash",
                schema: "workspace",
                table: "ResourceVersions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PlatformExtensionId",
                schema: "workspace",
                table: "ResourceItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelativePath",
                schema: "workspace",
                table: "ResourceItems",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceVersionLocations_WorkspaceAgentId",
                schema: "workspace",
                table: "ResourceVersionLocations",
                column: "WorkspaceAgentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceItems_WorkspaceId_DisplayName",
                schema: "workspace",
                table: "ResourceItems",
                columns: new[] { "WorkspaceId", "DisplayName" });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceItems_WorkspaceId_RelativePath",
                schema: "workspace",
                table: "ResourceItems",
                columns: new[] { "WorkspaceId", "RelativePath" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ResourceVersionLocations_WorkspaceAgentId",
                schema: "workspace",
                table: "ResourceVersionLocations");

            migrationBuilder.DropIndex(
                name: "IX_ResourceItems_WorkspaceId_DisplayName",
                schema: "workspace",
                table: "ResourceItems");

            migrationBuilder.DropIndex(
                name: "IX_ResourceItems_WorkspaceId_RelativePath",
                schema: "workspace",
                table: "ResourceItems");

            migrationBuilder.DropColumn(
                name: "RelativePath",
                schema: "workspace",
                table: "ResourceItems");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                schema: "workspace",
                table: "ResourceItems",
                newName: "Name");

            migrationBuilder.AlterColumn<string>(
                name: "FileHash",
                schema: "workspace",
                table: "ResourceVersions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "RelativePath",
                schema: "workspace",
                table: "ResourceVersionLocations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlatformExtensionId",
                schema: "workspace",
                table: "ResourceItems",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                schema: "workspace",
                table: "ResourceItems",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                schema: "workspace",
                table: "ResourceItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ResourceVersionLocations_WorkspaceAgentId_RelativePath",
                schema: "workspace",
                table: "ResourceVersionLocations",
                columns: new[] { "WorkspaceAgentId", "RelativePath" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceItems_ProjectId",
                schema: "workspace",
                table: "ResourceItems",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceItems_WorkspaceId",
                schema: "workspace",
                table: "ResourceItems",
                column: "WorkspaceId");
        }
    }
}
