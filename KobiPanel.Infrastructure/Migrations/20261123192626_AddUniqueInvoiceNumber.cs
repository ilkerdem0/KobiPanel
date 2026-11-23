using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiPanel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueInvoiceNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_BusinessId",
                table: "Invoices");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BusinessId_InvoiceNumber",
                table: "Invoices",
                columns: new[] { "BusinessId", "InvoiceNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_BusinessId_InvoiceNumber",
                table: "Invoices");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BusinessId",
                table: "Invoices",
                column: "BusinessId");
        }
    }
}
