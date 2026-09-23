using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Tag.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialLtreeTagSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tag");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:case_insensitive", "und-u-ks-level2,und-u-ks-level2,icu,False")
                .Annotation("Npgsql:PostgresExtension:ltree", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "TagItems",
                schema: "tag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Path = table.Column<string>(type: "ltree", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagItems_TagItems_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "tag",
                        principalTable: "TagItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TagLinks",
                schema: "tag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagPath = table.Column<string>(type: "ltree", nullable: false),
                    TargetSubPath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagLinks_TagItems_TagId",
                        column: x => x.TagId,
                        principalSchema: "tag",
                        principalTable: "TagItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagItems_ParentId",
                schema: "tag",
                table: "TagItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_TagItems_Path",
                schema: "tag",
                table: "TagItems",
                column: "Path")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_TagItems_ProjectId_Path",
                schema: "tag",
                table: "TagItems",
                columns: new[] { "ProjectId", "Path" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TagLinks_EntityType_EntityId",
                schema: "tag",
                table: "TagLinks",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_TagLinks_ProjectId",
                schema: "tag",
                table: "TagLinks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TagLinks_ProjectId_EntityType_EntityId",
                schema: "tag",
                table: "TagLinks",
                columns: new[] { "ProjectId", "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_TagLinks_TagId",
                schema: "tag",
                table: "TagLinks",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_TagLinks_TagPath",
                schema: "tag",
                table: "TagLinks",
                column: "TagPath")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagLinks",
                schema: "tag");

            migrationBuilder.DropTable(
                name: "TagItems",
                schema: "tag");
        }
    }
}
