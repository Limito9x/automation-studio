using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Pipeline.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPipelineExecutionBoundaryRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Kind",
                schema: "pipeline",
                table: "PipelineEdges",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "Progress",
                schema: "pipeline",
                table: "NodeExecutions",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb");

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                schema: "pipeline",
                table: "NodeExecutions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FinishedAt",
                schema: "pipeline",
                table: "NodeExecutions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "Log",
                schema: "pipeline",
                table: "NodeExecutions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "Output",
                schema: "pipeline",
                table: "NodeExecutions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartedAt",
                schema: "pipeline",
                table: "NodeExecutions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PipelineInputs",
                schema: "pipeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PipelineId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Cardinality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_PipelineInputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineInputs_Pipelines_PipelineId",
                        column: x => x.PipelineId,
                        principalSchema: "pipeline",
                        principalTable: "Pipelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PipelineInputs_PipelineId_Key",
                schema: "pipeline",
                table: "PipelineInputs",
                columns: new[] { "PipelineId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PipelineInputs",
                schema: "pipeline");

            migrationBuilder.DropColumn(
                name: "Kind",
                schema: "pipeline",
                table: "PipelineEdges");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                schema: "pipeline",
                table: "NodeExecutions");

            migrationBuilder.DropColumn(
                name: "FinishedAt",
                schema: "pipeline",
                table: "NodeExecutions");

            migrationBuilder.DropColumn(
                name: "Log",
                schema: "pipeline",
                table: "NodeExecutions");

            migrationBuilder.DropColumn(
                name: "Output",
                schema: "pipeline",
                table: "NodeExecutions");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                schema: "pipeline",
                table: "NodeExecutions");

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "Progress",
                schema: "pipeline",
                table: "NodeExecutions",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
