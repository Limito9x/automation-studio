using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Agent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameAgentPlatformConfigToExecutorConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentPlatformConfigs",
                schema: "agent");

            migrationBuilder.CreateTable(
                name: "AgentExecutorConfigs",
                schema: "agent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutorKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExecutablePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_AgentExecutorConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentExecutorConfigs_Agents_AgentId",
                        column: x => x.AgentId,
                        principalSchema: "agent",
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentExecutorConfigs_AgentId_ExecutorKey",
                schema: "agent",
                table: "AgentExecutorConfigs",
                columns: new[] { "AgentId", "ExecutorKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentExecutorConfigs",
                schema: "agent");

            migrationBuilder.CreateTable(
                name: "AgentPlatformConfigs",
                schema: "agent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    ExecutablePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    PlatformId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentPlatformConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentPlatformConfigs_Agents_AgentId",
                        column: x => x.AgentId,
                        principalSchema: "agent",
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentPlatformConfigs_AgentId_PlatformId",
                schema: "agent",
                table: "AgentPlatformConfigs",
                columns: new[] { "AgentId", "PlatformId" },
                unique: true);
        }
    }
}
