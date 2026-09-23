using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Files.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "files");

            migrationBuilder.CreateTable(
                name: "Assets",
                schema: "files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    OriginalName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Extension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HardHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssetLinks",
                schema: "files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerEntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SlotKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OwnerEntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AssetLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetLinks_Assets_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "files",
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetLinks_AssetId",
                schema: "files",
                table: "AssetLinks",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetLinks_OwnerEntityType_OwnerEntityId_SlotKey",
                schema: "files",
                table: "AssetLinks",
                columns: new[] { "OwnerEntityType", "OwnerEntityId", "SlotKey" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_HardHash",
                schema: "files",
                table: "Assets",
                column: "HardHash");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_StoragePath",
                schema: "files",
                table: "Assets",
                column: "StoragePath",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetLinks",
                schema: "files");

            migrationBuilder.DropTable(
                name: "Assets",
                schema: "files");
        }
    }
}



