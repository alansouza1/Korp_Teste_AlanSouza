using FaturamentoService.Application.DTOs.Requests;
using FaturamentoService.Application.DTOs.Responses;

namespace FaturamentoService.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceResponseDto> CreateAsync(CreateInvoiceRequestDto request, CancellationToken cancellationToken = default);
    Task<List<InvoiceResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InvoiceResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InvoiceResponseDto> GetBySequentialNumberAsync(int sequentialNumber, CancellationToken cancellationToken = default);
    Task<InvoiceResponseDto> AddItemsAsync(Guid invoiceId, AddInvoiceItemsRequestDto request, CancellationToken cancellationToken = default);
    Task<PrintInvoiceResponseDto> PrintAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IdempotentPrintResponseDto> PrintIdempotentAsync(Guid invoiceId, string idempotencyKey, CancellationToken cancellationToken = default);
}
