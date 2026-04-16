using FaturamentoService.Domain.Enums;
using FaturamentoService.Domain.Exceptions;

namespace FaturamentoService.Domain.Entities;

public class Invoice
{
    private readonly List<InvoiceItem> _items = [];

    public Guid Id { get; private set; }
    public int SequentialNumber { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public int PrintAttempts { get; private set; }
    public string? LastPrintError { get; private set; }
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    private Invoice()
    {
    }

    public static Invoice Create()
    {
        return new Invoice
        {
            Id = Guid.NewGuid(),
            Status = InvoiceStatus.Open,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(string productCode, string productDescription, int quantity)
    {
        EnsureIsOpen("Items can only be added to OPEN invoices.");
        _items.Add(new InvoiceItem(Id, productCode, productDescription, quantity));
        Touch();
    }

    public void RegisterPrintAttempt()
    {
        EnsureIsOpen("Only OPEN invoices can be printed.");
        PrintAttempts++;
        Touch();
    }

    public void RegisterPrintFailure(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new DomainException("Print error message is required.");
        }

        LastPrintError = message.Trim();
        Touch();
    }

    public void Close()
    {
        EnsureIsOpen("Only OPEN invoices can be printed.");
        Status = InvoiceStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        LastPrintError = null;
        Touch();
    }

    public void ClearPrintError()
    {
        LastPrintError = null;
        Touch();
    }

    private void EnsureIsOpen(string message)
    {
        if (Status != InvoiceStatus.Open)
        {
            throw new DomainException(message);
        }
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
