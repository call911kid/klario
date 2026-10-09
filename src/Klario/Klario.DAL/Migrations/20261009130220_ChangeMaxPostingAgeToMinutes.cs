using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Klario.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMaxPostingAgeToMinutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPostingAge",
                table: "SearchProfiles");

            migrationBuilder.AlterColumn<int>(
                name: "IntervalMinutes",
                table: "SearchProfiles",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 15);

            migrationBuilder.AddColumn<int>(
                name: "MaxPostingAgeMinutes",
                table: "SearchProfiles",
                type: "int",
                nullable: false,
                defaultValue: 1440);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPostingAgeMinutes",
                table: "SearchProfiles");

            migrationBuilder.AlterColumn<int>(
                name: "IntervalMinutes",
                table: "SearchProfiles",
                type: "int",
                nullable: false,
                defaultValue: 15,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MaxPostingAge",
                table: "SearchProfiles",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
