using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Klario.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    SearchProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IntervalMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 15),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => new { x.SearchProfileId, x.Source });
                    table.ForeignKey(
                        name: "FK_Schedules_SearchProfiles_SearchProfileId",
                        column: x => x.SearchProfileId,
                        principalTable: "SearchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Backfill existing search profile interval settings to Schedules for LinkedIn
            migrationBuilder.Sql(@"
                INSERT INTO Schedules (SearchProfileId, Source, IntervalMinutes, IsEnabled)
                SELECT 
                    Id,
                    'LinkedIn',
                    ISNULL(IntervalMinutes, 15),
                    1
                FROM SearchProfiles;
            ");

            migrationBuilder.DropColumn(
                name: "IntervalMinutes",
                table: "SearchProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IntervalMinutes",
                table: "SearchProfiles",
                type: "int",
                nullable: false,
                defaultValue: 15);

            migrationBuilder.Sql(@"
                UPDATE sp
                SET sp.IntervalMinutes = ISNULL(s.IntervalMinutes, 15)
                FROM SearchProfiles sp
                JOIN Schedules s ON s.SearchProfileId = sp.Id AND s.Source = 'LinkedIn';
            ");

            migrationBuilder.DropTable(
                name: "Schedules");
        }
    }
}
