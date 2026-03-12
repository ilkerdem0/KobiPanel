using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiPanel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Transactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsScheduled",
                table: "Transactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsScheduled",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Transactions");
        }
    }
}
