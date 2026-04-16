using EstoqueService.Domain.Exceptions;

namespace EstoqueService.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int StockQuantity { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Product()
    {
    }

    public Product(string code, string description, int stockQuantity)
    {
        ValidateCode(code);
        ValidateDescription(description);
        ValidateStockQuantity(stockQuantity);

        Id = Guid.NewGuid();
        Code = code.Trim().ToUpperInvariant();
        Description = description.Trim();
        StockQuantity = stockQuantity;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string description)
    {
        ValidateDescription(description);
        Description = description.Trim();
        Touch();
    }

    public void SetStock(int stockQuantity)
    {
        ValidateStockQuantity(stockQuantity);
        StockQuantity = stockQuantity;
        Touch();
    }

    public void DebitStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Debit quantity must be greater than zero.");
        }

        if (StockQuantity < quantity)
        {
            throw new DomainException($"Insufficient stock for product '{Code}'.");
        }

        StockQuantity -= quantity;
        Touch();
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Product code is required.");
        }
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Product description is required.");
        }
    }

    private static void ValidateStockQuantity(int stockQuantity)
    {
        if (stockQuantity < 0)
        {
            throw new DomainException("Stock quantity cannot be negative.");
        }
    }
}
