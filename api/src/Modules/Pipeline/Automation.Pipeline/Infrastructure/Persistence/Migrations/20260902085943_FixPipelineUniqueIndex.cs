using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Pipeline.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPipelineUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pipelines_ProjectId_Name",
                schema: "pipeline",
                table: "Pipelines");

            migrationBuilder.CreateIndex(
                name: "IX_Pipelines_ProjectId_Name",
                schema: "pipeline",
                table: "Pipelines",
                columns: new[] { "ProjectId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pipelines_ProjectId_Name",
                schema: "pipeline",
                table: "Pipelines");

            migrationBuilder.CreateIndex(
                name: "IX_Pipelines_ProjectId_Name",
                schema: "pipeline",
                table: "Pipelines",
                columns: new[] { "ProjectId", "Name" },
                unique: true);
        }
    }
}
