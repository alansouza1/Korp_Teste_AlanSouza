using FaturamentoService.Application.DTOs.Responses;

namespace FaturamentoService.Application.Interfaces;

public interface IStockServiceClient
{
    Task<StockValidationResultDto> ValidateStockAsync(IEnumerable<(string ProductCode, int Quantity)> items, CancellationToken cancellationToken = default);
    Task DebitStockAsync(IEnumerable<(string ProductCode, int Quantity)> items, CancellationToken cancellationToken = default);
}
