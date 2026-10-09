using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Klario.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIntervalMinutesToSearchProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IntervalMinutes",
                table: "SearchProfiles",
                type: "int",
                nullable: false,
                defaultValue: 15);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntervalMinutes",
                table: "SearchProfiles");
        }
    }
}
