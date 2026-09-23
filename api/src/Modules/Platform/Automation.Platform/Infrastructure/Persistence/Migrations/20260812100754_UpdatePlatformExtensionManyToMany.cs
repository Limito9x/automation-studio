using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlatformExtensionManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlatformExtensions_Platforms_PlatformId",
                schema: "platform",
                table: "PlatformExtensions");

            migrationBuilder.DropIndex(
                name: "IX_PlatformExtensions_PlatformId_Extension",
                schema: "platform",
                table: "PlatformExtensions");

            migrationBuilder.DropColumn(
                name: "PlatformId",
                schema: "platform",
                table: "PlatformExtensions");

            migrationBuilder.CreateTable(
                name: "PlatformPlatformExtension",
                schema: "platform",
                columns: table => new
                {
                    ExtensionsId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformPlatformExtension", x => new { x.ExtensionsId, x.PlatformsId });
                    table.ForeignKey(
                        name: "FK_PlatformPlatformExtension_PlatformExtensions_ExtensionsId",
                        column: x => x.ExtensionsId,
                        principalSchema: "platform",
                        principalTable: "PlatformExtensions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlatformPlatformExtension_Platforms_PlatformsId",
                        column: x => x.PlatformsId,
                        principalSchema: "platform",
                        principalTable: "Platforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformExtensions_Extension",
                schema: "platform",
                table: "PlatformExtensions",
                column: "Extension",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPlatformExtension_PlatformsId",
                schema: "platform",
                table: "PlatformPlatformExtension",
                column: "PlatformsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformPlatformExtension",
                schema: "platform");

            migrationBuilder.DropIndex(
                name: "IX_PlatformExtensions_Extension",
                schema: "platform",
                table: "PlatformExtensions");

            migrationBuilder.AddColumn<Guid>(
                name: "PlatformId",
                schema: "platform",
                table: "PlatformExtensions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PlatformExtensions_PlatformId_Extension",
                schema: "platform",
                table: "PlatformExtensions",
                columns: new[] { "PlatformId", "Extension" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformExtensions_Platforms_PlatformId",
                schema: "platform",
                table: "PlatformExtensions",
                column: "PlatformId",
                principalSchema: "platform",
                principalTable: "Platforms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

