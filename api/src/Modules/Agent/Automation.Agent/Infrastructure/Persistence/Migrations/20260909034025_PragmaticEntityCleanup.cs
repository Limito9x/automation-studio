using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Agent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PragmaticEntityCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "agent",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "agent",
                table: "AgentExecutorConfigs");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "agent",
                table: "AgentExecutorConfigs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "agent",
                table: "AgentExecutorConfigs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "agent",
                table: "Agents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                schema: "agent",
                table: "AgentExecutorConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "agent",
                table: "AgentExecutorConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "agent",
                table: "AgentExecutorConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
