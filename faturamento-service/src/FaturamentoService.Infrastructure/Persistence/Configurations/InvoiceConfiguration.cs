using FaturamentoService.Domain.Entities;
using FaturamentoService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaturamentoService.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(x => x.SequentialNumber)
            .HasColumnName("sequential_number")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("nextval('invoice_numbers')");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(
                status => status == InvoiceStatus.Open ? "OPEN" : "CLOSED",
                value => value == "CLOSED" ? InvoiceStatus.Closed : InvoiceStatus.Open)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(x => x.ClosedAt)
            .HasColumnName("closed_at");

        builder.Property(x => x.PrintAttempts)
            .HasColumnName("print_attempts")
            .IsRequired();

        builder.Property(x => x.LastPrintError)
            .HasColumnName("last_print_error")
            .HasMaxLength(1000);

        builder.Metadata
            .FindNavigation(nameof(Invoice.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SequentialNumber)
            .IsUnique();
    }
}
