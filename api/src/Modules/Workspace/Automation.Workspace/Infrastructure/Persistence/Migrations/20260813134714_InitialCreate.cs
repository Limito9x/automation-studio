using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Workspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "workspace");

            migrationBuilder.CreateTable(
                name: "Workspaces",
                schema: "workspace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResourceItems",
                schema: "workspace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PlatformExtensionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceItems_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "workspace",
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceAgents",
                schema: "workspace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RootPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceAgents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkspaceAgents_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "workspace",
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkspacePlatforms",
                schema: "workspace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspacePlatforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkspacePlatforms_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "workspace",
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceVersions",
                schema: "workspace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNo = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceVersions_ResourceItems_ResourceId",
                        column: x => x.ResourceId,
                        principalSchema: "workspace",
                        principalTable: "ResourceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceVersionLocations",
                schema: "workspace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceAgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelativePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsOrigin = table.Column<bool>(type: "boolean", nullable: false),
                    DiscoveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceVersionLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceVersionLocations_ResourceVersions_ResourceVersionId",
                        column: x => x.ResourceVersionId,
                        principalSchema: "workspace",
                        principalTable: "ResourceVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceVersionLocations_WorkspaceAgents_WorkspaceAgentId",
                        column: x => x.WorkspaceAgentId,
                        principalSchema: "workspace",
                        principalTable: "WorkspaceAgents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceItems_ContentId",
                schema: "workspace",
                table: "ResourceItems",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceItems_PlatformExtensionId",
                schema: "workspace",
                table: "ResourceItems",
                column: "PlatformExtensionId");

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

            migrationBuilder.CreateIndex(
                name: "IX_ResourceVersionLocations_ResourceVersionId",
                schema: "workspace",
                table: "ResourceVersionLocations",
                column: "ResourceVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceVersionLocations_WorkspaceAgentId_RelativePath",
                schema: "workspace",
                table: "ResourceVersionLocations",
                columns: new[] { "WorkspaceAgentId", "RelativePath" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceVersions_ResourceId_FileHash",
                schema: "workspace",
                table: "ResourceVersions",
                columns: new[] { "ResourceId", "FileHash" },
                unique: true,
                filter: "\"FileHash\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceVersions_ResourceId_VersionNo",
                schema: "workspace",
                table: "ResourceVersions",
                columns: new[] { "ResourceId", "VersionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceAgents_WorkspaceId_AgentId",
                schema: "workspace",
                table: "WorkspaceAgents",
                columns: new[] { "WorkspaceId", "AgentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspacePlatforms_WorkspaceId_PlatformId",
                schema: "workspace",
                table: "WorkspacePlatforms",
                columns: new[] { "WorkspaceId", "PlatformId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourceVersionLocations",
                schema: "workspace");

            migrationBuilder.DropTable(
                name: "WorkspacePlatforms",
                schema: "workspace");

            migrationBuilder.DropTable(
                name: "ResourceVersions",
                schema: "workspace");

            migrationBuilder.DropTable(
                name: "WorkspaceAgents",
                schema: "workspace");

            migrationBuilder.DropTable(
                name: "ResourceItems",
                schema: "workspace");

            migrationBuilder.DropTable(
                name: "Workspaces",
                schema: "workspace");
        }
    }
}
