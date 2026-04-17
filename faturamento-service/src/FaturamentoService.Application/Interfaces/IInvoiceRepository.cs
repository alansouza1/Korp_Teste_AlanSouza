using FaturamentoService.Domain.Entities;

namespace FaturamentoService.Application.Interfaces;

public interface IInvoiceRepository
{
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task<List<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetBySequentialNumberAsync(int sequentialNumber, CancellationToken cancellationToken = default);
    Task<InvoicePrintIdempotencyRecord?> GetPrintIdempotencyRecordAsync(Guid invoiceId, string idempotencyKey, CancellationToken cancellationToken = default);
    Task<bool> TryCreatePrintIdempotencyRecordAsync(InvoicePrintIdempotencyRecord record, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
