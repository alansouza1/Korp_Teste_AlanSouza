using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FaturamentoService.Application.DTOs.Responses;
using FaturamentoService.Application.Exceptions;
using FaturamentoService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FaturamentoService.Infrastructure.Clients;

public class StockServiceClient : IStockServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StockServiceClient> _logger;

    public StockServiceClient(HttpClient httpClient, ILogger<StockServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<StockValidationResultDto> ValidateStockAsync(IEnumerable<(string ProductCode, int Quantity)> items, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new
            {
                items = items.Select(x => new
                {
                    productCode = x.ProductCode,
                    quantity = x.Quantity
                })
            };

            using var response = await _httpClient.PostAsJsonAsync("/api/products/stock/validate", request, cancellationToken);
            return await HandleValidationResponseAsync(response, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "Stock validation request failed");
            throw new ExternalServiceException("Unable to communicate with stock service right now.");
        }
    }

    public async Task DebitStockAsync(IEnumerable<(string ProductCode, int Quantity)> items, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new
            {
                items = items.Select(x => new
                {
                    productCode = x.ProductCode,
                    quantity = x.Quantity
                })
            };

            using var response = await _httpClient.PostAsJsonAsync("/api/products/stock/debit", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var error = await ReadErrorAsync(response, cancellationToken);
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogWarning("Stock debit unavailable: {Message}", error.Message);
                throw new ExternalServiceException(error.Message);
            }

            throw new ValidationException(error.Message);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "Stock debit request failed");
            throw new ExternalServiceException("Unable to communicate with stock service right now.");
        }
    }

    private async Task<StockValidationResultDto> HandleValidationResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<StockValidationApiResponse>(cancellationToken: cancellationToken);
            if (result is null)
            {
                throw new ExternalServiceException("Invalid response from stock service.");
            }

            return new StockValidationResultDto
            {
                IsValid = result.IsValid,
                Items = result.Items.Select(x => new StockItemAvailabilityDto
                {
                    ProductCode = x.ProductCode,
                    RequestedQuantity = x.RequestedQuantity,
                    AvailableQuantity = x.AvailableQuantity,
                    IsAvailable = x.IsAvailable,
                    Message = x.Message
                }).ToList()
            };
        }

        var error = await ReadErrorAsync(response, cancellationToken);
        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            _logger.LogWarning("Stock validation unavailable: {Message}", error.Message);
            throw new ExternalServiceException(error.Message);
        }

        throw new ValidationException(error.Message);
    }

    private static async Task<ErrorResponseDto> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(cancellationToken: cancellationToken);
            if (payload is not null && !string.IsNullOrWhiteSpace(payload.Message))
            {
                return payload;
            }
        }
        catch (JsonException)
        {
        }

        return new ErrorResponseDto
        {
            Message = "Unable to communicate with stock service."
        };
    }

    private class StockValidationApiResponse
    {
        public bool IsValid { get; set; }
        public List<StockValidationItemApiResponse> Items { get; set; } = [];
    }

    private class StockValidationItemApiResponse
    {
        public string ProductCode { get; set; } = string.Empty;
        public int RequestedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    private class ErrorResponseDto
    {
        public string Message { get; set; } = string.Empty;
    }
}
