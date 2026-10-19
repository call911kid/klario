using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Klario.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalizedNameToExcludedCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExcludedCompanies_Name",
                table: "ExcludedCompanies");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "ExcludedCompanies",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE ExcludedCompanies SET NormalizedName = LOWER(LTRIM(RTRIM(Name))) WHERE NormalizedName = '' OR NormalizedName IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_ExcludedCompanies_NormalizedName",
                table: "ExcludedCompanies",
                column: "NormalizedName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExcludedCompanies_NormalizedName",
                table: "ExcludedCompanies");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "ExcludedCompanies");

            migrationBuilder.CreateIndex(
                name: "IX_ExcludedCompanies_Name",
                table: "ExcludedCompanies",
                column: "Name",
                unique: true);
        }
    }
}
