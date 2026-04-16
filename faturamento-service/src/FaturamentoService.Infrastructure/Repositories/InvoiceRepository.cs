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
        if (string.Equals(_context.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal))
        {
            var sequentialNumbers = await _context.Invoices
                .Select(x => x.SequentialNumber)
                .ToListAsync(cancellationToken);

            var maxSequentialNumber = sequentialNumbers.Any() ? sequentialNumbers.Max() : 0;
            var nextSequentialNumber = maxSequentialNumber + 1;

            invoice.AssignSequentialNumber(nextSequentialNumber);
        }

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

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (invoice is not null && _context.Entry(invoice).State == EntityState.Detached)
        {
            _context.Invoices.Attach(invoice);
        }

        return invoice;
    }

    public Task<Invoice?> GetBySequentialNumberAsync(int sequentialNumber, CancellationToken cancellationToken = default)
    {
        return _context.Invoices
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.SequentialNumber == sequentialNumber, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (string.Equals(_context.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal))
        {
            _context.ChangeTracker.DetectChanges();

            var persistedItemIds = (await _context.InvoiceItems
                .AsNoTracking()
                .Select(x => x.Id)
                .ToListAsync(cancellationToken))
                .ToHashSet();

            var trackedInvoices = _context.ChangeTracker.Entries<Invoice>()
                .Where(entry => entry.State != EntityState.Detached)
                .Select(entry => entry.Entity)
                .ToList();

            foreach (var invoice in trackedInvoices)
            {
                foreach (var item in invoice.Items)
                {
                    var itemEntry = _context.Entry(item);
                    if (!persistedItemIds.Contains(item.Id) && itemEntry.State != EntityState.Added)
                    {
                        itemEntry.State = EntityState.Added;
                    }
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
