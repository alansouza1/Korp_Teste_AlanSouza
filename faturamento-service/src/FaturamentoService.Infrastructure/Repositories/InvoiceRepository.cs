using FaturamentoService.Application.Interfaces;
using FaturamentoService.Domain.Entities;
using FaturamentoService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FaturamentoService.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly FaturamentoDbContext _context;

    public InvoiceRepository(FaturamentoDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        await _context.Invoices.AddAsync(invoice, cancellationToken);
    }

    public Task<List<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _context.Invoices
            .AsNoTracking()
            .Include(x => x.Items)
            .OrderByDescending(x => x.SequentialNumber)
            .ToListAsync(cancellationToken);
    }

    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Invoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Invoice?> GetBySequentialNumberAsync(int sequentialNumber, CancellationToken cancellationToken = default)
    {
        return _context.Invoices
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.SequentialNumber == sequentialNumber, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
