using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReminderAssistantBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProcessingLease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reminders_Status_DueAtUtc",
                table: "Reminders");

            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "Reminders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "LeaseToken",
                table: "Reminders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LeaseUntilUtc",
                table: "Reminders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_Status_DueAtUtc_LeaseUntilUtc",
                table: "Reminders",
                columns: new[] { "Status", "DueAtUtc", "LeaseUntilUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reminders_Status_DueAtUtc_LeaseUntilUtc",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "LeaseToken",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "LeaseUntilUtc",
                table: "Reminders");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_Status_DueAtUtc",
                table: "Reminders",
                columns: new[] { "Status", "DueAtUtc" });
        }
    }
}
