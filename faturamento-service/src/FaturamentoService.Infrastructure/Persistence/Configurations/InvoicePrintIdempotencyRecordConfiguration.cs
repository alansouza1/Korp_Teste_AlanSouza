using FaturamentoService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaturamentoService.Infrastructure.Persistence.Configurations;

public class InvoicePrintIdempotencyRecordConfiguration : IEntityTypeConfiguration<InvoicePrintIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<InvoicePrintIdempotencyRecord> builder)
    {
        builder.ToTable("invoice_print_idempotency_records");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(x => x.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired();

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ResponseStatusCode)
            .HasColumnName("response_status_code")
            .IsRequired();

        builder.Property(x => x.ResponseJson)
            .HasColumnName("response_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => new { x.InvoiceId, x.IdempotencyKey })
            .IsUnique();
    }
}
