using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Pipeline.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PragmaticEntityCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "pipeline",
                table: "Pipelines");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "pipeline",
                table: "PipelineOutputs");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "pipeline",
                table: "PipelineOutputs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "pipeline",
                table: "PipelineOutputs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "pipeline",
                table: "PipelineNodes");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "pipeline",
                table: "PipelineNodes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "pipeline",
                table: "PipelineNodes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "pipeline",
                table: "PipelineInputs");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "pipeline",
                table: "PipelineInputs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "pipeline",
                table: "PipelineInputs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "pipeline",
                table: "PipelineExecutions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "pipeline",
                table: "PipelineEdges");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "pipeline",
                table: "PipelineEdges");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "pipeline",
                table: "PipelineEdges");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "pipeline",
                table: "NodeExecutions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "pipeline",
                table: "NodeExecutions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "pipeline",
                table: "NodeExecutions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "pipeline",
                table: "NodeDefinitions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "pipeline",
                table: "Pipelines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "pipeline",
                table: "PipelineOutputs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "pipeline",
                table: "PipelineOutputs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "pipeline",
                table: "PipelineOutputs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "pipeline",
                table: "PipelineNodes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "pipeline",
                table: "PipelineNodes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "pipeline",
                table: "PipelineNodes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "pipeline",
                table: "PipelineInputs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "pipeline",
                table: "PipelineInputs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "pipeline",
                table: "PipelineInputs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "pipeline",
                table: "PipelineExecutions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "pipeline",
                table: "PipelineEdges",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "pipeline",
                table: "PipelineEdges",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "pipeline",
                table: "PipelineEdges",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "pipeline",
                table: "NodeExecutions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "pipeline",
                table: "NodeExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "pipeline",
                table: "NodeExecutions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "pipeline",
                table: "NodeDefinitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
