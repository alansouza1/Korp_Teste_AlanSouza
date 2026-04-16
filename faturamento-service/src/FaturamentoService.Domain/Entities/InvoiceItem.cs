using FaturamentoService.Domain.Exceptions;

namespace FaturamentoService.Domain.Entities;

public class InvoiceItem
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string ProductDescription { get; private set; } = string.Empty;
    public int Quantity { get; private set; }

    private InvoiceItem()
    {
    }

    internal InvoiceItem(Guid invoiceId, string productCode, string productDescription, int quantity)
    {
        ValidateInvoiceId(invoiceId);
        ValidateProductCode(productCode);
        ValidateProductDescription(productDescription);
        ValidateQuantity(quantity);

        Id = Guid.NewGuid();
        InvoiceId = invoiceId;
        ProductCode = productCode.Trim().ToUpperInvariant();
        ProductDescription = productDescription.Trim();
        Quantity = quantity;
    }

    private static void ValidateInvoiceId(Guid invoiceId)
    {
        if (invoiceId == Guid.Empty)
        {
            throw new DomainException("Invoice id is required.");
        }
    }

    private static void ValidateProductCode(string productCode)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            throw new DomainException("Product code is required.");
        }
    }

    private static void ValidateProductDescription(string productDescription)
    {
        if (string.IsNullOrWhiteSpace(productDescription))
        {
            throw new DomainException("Product description is required.");
        }
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }
    }
}
