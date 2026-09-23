using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.DynamicForms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDependencySchemaIdsToSchemaVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<Guid>>(
                name: "DependencySchemaIds",
                schema: "dynamicforms",
                table: "SchemaVersions",
                type: "uuid[]",
                nullable: false,
                defaultValueSql: "'{}'::uuid[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DependencySchemaIds",
                schema: "dynamicforms",
                table: "SchemaVersions");
        }
    }
}
