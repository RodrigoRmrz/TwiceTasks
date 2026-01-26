using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using TwiceTasks.Data;

#nullable disable

namespace TwiceTasks.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260126060000_AddCalendarEventEndAllDay")]
    public partial class AddCalendarEventEndAllDay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllDay",
                table: "CalendarEvents",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "End",
                table: "CalendarEvents",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllDay",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "End",
                table: "CalendarEvents");
        }
    }
}
