using EstoqueService.Application.Interfaces;
using EstoqueService.Domain.Entities;
using EstoqueService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly EstoqueDbContext _context;

    public ProductRepository(EstoqueDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public async Task<List<Product>> GetAllAsync(string? code, string? description, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = _context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(code))
        {
            var normalizedCode = code.Trim().ToUpperInvariant();
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{normalizedCode}%"));
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            var normalizedDescription = description.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Description, $"%{normalizedDescription}%"));
        }

        return await query
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return _context.Products.FirstOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);
    }

    public Task<List<Product>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken cancellationToken = default)
    {
        var normalizedCodes = codes
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        return _context.Products
            .Where(x => normalizedCodes.Contains(x.Code))
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return _context.Products.AnyAsync(x => x.Code == normalizedCode, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (string.Equals(_context.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal))
        {
            return new NoOpTransaction();
        }

        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new EfCoreTransactionWrapper(transaction);
    }
}
