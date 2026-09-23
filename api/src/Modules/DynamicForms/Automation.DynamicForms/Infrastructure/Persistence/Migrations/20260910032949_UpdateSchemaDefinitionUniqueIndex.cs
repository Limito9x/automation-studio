using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.DynamicForms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaDefinitionUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchemaDefinitions_OwnerId_OwnerType",
                schema: "dynamicforms",
                table: "SchemaDefinitions");

            migrationBuilder.CreateIndex(
                name: "IX_SchemaDefinitions_OwnerId_OwnerType_Name",
                schema: "dynamicforms",
                table: "SchemaDefinitions",
                columns: new[] { "OwnerId", "OwnerType", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchemaDefinitions_OwnerId_OwnerType_Name",
                schema: "dynamicforms",
                table: "SchemaDefinitions");

            migrationBuilder.CreateIndex(
                name: "IX_SchemaDefinitions_OwnerId_OwnerType",
                schema: "dynamicforms",
                table: "SchemaDefinitions",
                columns: new[] { "OwnerId", "OwnerType" },
                unique: true);
        }
    }
}
