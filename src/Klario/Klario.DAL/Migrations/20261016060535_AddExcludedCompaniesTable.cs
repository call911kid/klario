using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Klario.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddExcludedCompaniesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExcludedCompanies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcludedCompanies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExcludedCompanies_Name",
                table: "ExcludedCompanies",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExcludedCompanies");
        }
    }
}
