using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FaturamentoService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(Context.FaturamentoDbContext))]
[Migration("20260417031000_AddInvoicePrintIdempotency")]
public partial class AddInvoicePrintIdempotency : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "invoice_print_idempotency_records",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                response_status_code = table.Column<int>(type: "integer", nullable: false),
                response_json = table.Column<string>(type: "jsonb", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_invoice_print_idempotency_records", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_invoice_print_idempotency_records_invoice_id_idempotency_key",
            table: "invoice_print_idempotency_records",
            columns: new[] { "invoice_id", "idempotency_key" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "invoice_print_idempotency_records");
    }
}
