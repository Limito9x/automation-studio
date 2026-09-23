using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Workspace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixResourceLocationUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ResourceVersionLocations_ResourceVersionId",
                schema: "workspace",
                table: "ResourceVersionLocations");

            migrationBuilder.DropIndex(
                name: "IX_ResourceVersionLocations_WorkspaceAgentId",
                schema: "workspace",
                table: "ResourceVersionLocations");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceVersionLocations_ResourceVersionId_WorkspaceAgentId",
                schema: "workspace",
                table: "ResourceVersionLocations",
                columns: new[] { "ResourceVersionId", "WorkspaceAgentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceVersionLocations_WorkspaceAgentId",
                schema: "workspace",
                table: "ResourceVersionLocations",
                column: "WorkspaceAgentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ResourceVersionLocations_ResourceVersionId_WorkspaceAgentId",
                schema: "workspace",
                table: "ResourceVersionLocations");

            migrationBuilder.DropIndex(
                name: "IX_ResourceVersionLocations_WorkspaceAgentId",
                schema: "workspace",
                table: "ResourceVersionLocations");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceVersionLocations_ResourceVersionId",
                schema: "workspace",
                table: "ResourceVersionLocations",
                column: "ResourceVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceVersionLocations_WorkspaceAgentId",
                schema: "workspace",
                table: "ResourceVersionLocations",
                column: "WorkspaceAgentId",
                unique: true);
        }
    }
}
