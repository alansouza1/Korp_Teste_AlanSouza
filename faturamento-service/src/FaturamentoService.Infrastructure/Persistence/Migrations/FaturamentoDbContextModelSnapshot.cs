using System;
using FaturamentoService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace FaturamentoService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(FaturamentoDbContext))]
partial class FaturamentoDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasSequence<int>("invoice_numbers")
            .HasAnnotation("ProductVersion", "8.0.8")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("FaturamentoService.Domain.Entities.Invoice", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid")
                .HasColumnName("id");

            b.Property<DateTime?>("ClosedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("closed_at");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");

            b.Property<string>("LastPrintError")
                .HasMaxLength(1000)
                .HasColumnType("character varying(1000)")
                .HasColumnName("last_print_error");

            b.Property<int>("PrintAttempts")
                .HasColumnType("integer")
                .HasColumnName("print_attempts");

            b.Property<int>("SequentialNumber")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasDefaultValueSql("nextval('invoice_numbers')")
                .HasColumnName("sequential_number");

            b.Property<string>("Status")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)")
                .HasColumnName("status");

            b.Property<DateTime>("UpdatedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("updated_at");

            b.HasKey("Id");

            b.HasIndex("SequentialNumber")
                .IsUnique();

            b.ToTable("invoices", (string?)null);
        });

        modelBuilder.Entity("FaturamentoService.Domain.Entities.InvoiceItem", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid")
                .HasColumnName("id");

            b.Property<Guid>("InvoiceId")
                .HasColumnType("uuid")
                .HasColumnName("invoice_id");

            b.Property<string>("ProductCode")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("character varying(50)")
                .HasColumnName("product_code");

            b.Property<string>("ProductDescription")
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("character varying(255)")
                .HasColumnName("product_description");

            b.Property<int>("Quantity")
                .HasColumnType("integer")
                .HasColumnName("quantity");

            b.HasKey("Id");

            b.HasIndex("InvoiceId");

            b.ToTable("invoice_items", (string?)null);
        });

        modelBuilder.Entity("FaturamentoService.Domain.Entities.InvoiceItem", b =>
        {
            b.HasOne("FaturamentoService.Domain.Entities.Invoice", null)
                .WithMany("Items")
                .HasForeignKey("InvoiceId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("FaturamentoService.Domain.Entities.Invoice", b =>
        {
            b.Navigation("Items");
        });
#pragma warning restore 612, 618
    }
}
