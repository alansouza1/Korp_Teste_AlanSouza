using FaturamentoService.Application.DTOs.Responses;
using FaturamentoService.Application.Interfaces;

namespace FaturamentoService.IntegrationTests.Infrastructure;

public class FakeStockServiceClient : IStockServiceClient
{
    public Func<IEnumerable<(string ProductCode, int Quantity)>, CancellationToken, Task<StockValidationResultDto>>? ValidateHandler { get; set; }
    public Func<IEnumerable<(string ProductCode, int Quantity)>, CancellationToken, Task>? DebitHandler { get; set; }
    public int DebitCalls { get; private set; }

    public Task<StockValidationResultDto> ValidateStockAsync(IEnumerable<(string ProductCode, int Quantity)> items, CancellationToken cancellationToken = default)
    {
        if (ValidateHandler is not null)
        {
            return ValidateHandler(items, cancellationToken);
        }

        return Task.FromResult(new StockValidationResultDto
        {
            IsValid = true,
            Items = items.Select(x => new StockItemAvailabilityDto
            {
                ProductCode = x.ProductCode,
                RequestedQuantity = x.Quantity,
                AvailableQuantity = x.Quantity,
                IsAvailable = true,
                Message = "Stock available."
            }).ToList()
        });
    }

    public Task DebitStockAsync(IEnumerable<(string ProductCode, int Quantity)> items, CancellationToken cancellationToken = default)
    {
        DebitCalls++;

        if (DebitHandler is not null)
        {
            return DebitHandler(items, cancellationToken);
        }

        return Task.CompletedTask;
    }
}
