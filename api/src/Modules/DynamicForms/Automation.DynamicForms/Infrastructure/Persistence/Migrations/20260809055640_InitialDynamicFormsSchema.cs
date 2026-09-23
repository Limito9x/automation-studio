using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.DynamicForms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialDynamicFormsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dynamicforms");

            migrationBuilder.CreateTable(
                name: "SchemaDefinitions",
                schema: "dynamicforms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OwnerType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_SchemaDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchemaVersions",
                schema: "dynamicforms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchemaDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fields = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_SchemaVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchemaVersions_SchemaDefinitions_SchemaDefinitionId",
                        column: x => x.SchemaDefinitionId,
                        principalSchema: "dynamicforms",
                        principalTable: "SchemaDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchemaData",
                schema: "dynamicforms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchemaVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Values = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    ClientId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClientType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_SchemaData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchemaData_SchemaVersions_SchemaVersionId",
                        column: x => x.SchemaVersionId,
                        principalSchema: "dynamicforms",
                        principalTable: "SchemaVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchemaData_ClientId_ClientType",
                schema: "dynamicforms",
                table: "SchemaData",
                columns: new[] { "ClientId", "ClientType" });

            migrationBuilder.CreateIndex(
                name: "IX_SchemaData_SchemaVersionId",
                schema: "dynamicforms",
                table: "SchemaData",
                column: "SchemaVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SchemaDefinitions_OwnerId_OwnerType",
                schema: "dynamicforms",
                table: "SchemaDefinitions",
                columns: new[] { "OwnerId", "OwnerType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchemaVersions_SchemaDefinitionId",
                schema: "dynamicforms",
                table: "SchemaVersions",
                column: "SchemaDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchemaData",
                schema: "dynamicforms");

            migrationBuilder.DropTable(
                name: "SchemaVersions",
                schema: "dynamicforms");

            migrationBuilder.DropTable(
                name: "SchemaDefinitions",
                schema: "dynamicforms");
        }
    }
}

