using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FaturamentoService.Infrastructure.Persistence.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateSequence<int>(
            name: "invoice_numbers");

        migrationBuilder.CreateTable(
            name: "invoices",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                sequential_number = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('invoice_numbers')"),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                print_attempts = table.Column<int>(type: "integer", nullable: false),
                last_print_error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_invoices", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "invoice_items",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                product_description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_invoice_items", x => x.id);
                table.ForeignKey(
                    name: "FK_invoice_items_invoices_invoice_id",
                    column: x => x.invoice_id,
                    principalTable: "invoices",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_invoice_items_invoice_id",
            table: "invoice_items",
            column: "invoice_id");

        migrationBuilder.CreateIndex(
            name: "IX_invoices_sequential_number",
            table: "invoices",
            column: "sequential_number",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "invoice_items");

        migrationBuilder.DropTable(
            name: "invoices");

        migrationBuilder.DropSequence(
            name: "invoice_numbers");
    }
}
