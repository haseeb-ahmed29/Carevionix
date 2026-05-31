using System;
using Carevionix.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carevionix.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260531023000_AddDemoInvoicePayments")]
    public partial class AddDemoInvoicePayments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DemoPayerName",
                table: "Invoices",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DemoPaymentMethod",
                table: "Invoices",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DemoPaymentReceipt",
                table: "Invoices",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DemoPaymentSubmittedAt",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentApprovedAt",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DemoPayerName",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DemoPaymentMethod",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DemoPaymentReceipt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DemoPaymentSubmittedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentApprovedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Invoices");
        }
    }
}
