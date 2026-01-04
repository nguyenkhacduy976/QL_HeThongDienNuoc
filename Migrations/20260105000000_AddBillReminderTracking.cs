using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QL_HethongDiennuoc.Data;

#nullable disable

namespace QL_HethongDiennuoc.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260105000000_AddBillReminderTracking")]
    /// <inheritdoc />
    public partial class AddBillReminderTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastReminderSent",
                table: "Bills",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReminderCount",
                table: "Bills",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastReminderSent",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "ReminderCount",
                table: "Bills");
        }
    }
}
