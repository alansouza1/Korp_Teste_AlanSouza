using System.Net;
using System.Net.Http.Json;
using EstoqueService.IntegrationTests.Infrastructure;
using Xunit;

namespace EstoqueService.IntegrationTests;

public class ProductConcurrencyTests
{
    [Fact]
    public async Task ConcurrentDebit_ForLastUnit_ShouldAllowOnlyOneSuccess()
    {
        if (!EstoquePostgresApiFactory.IsEnabled)
        {
            return;
        }

        await using var factory = new EstoquePostgresApiFactory();
        using var clientA = factory.CreateClient();
        using var clientB = factory.CreateClient();

        var createResponse = await clientA.PostAsJsonAsync("/api/products", new
        {
            code = "P-CONCURRENT-001",
            description = "Produto Concorrente",
            stockQuantity = 1
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var startSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<HttpResponseMessage> DebitAsync(HttpClient client) =>
            Task.Run(async () =>
            {
                await startSignal.Task;
                return await client.PostAsJsonAsync("/api/products/stock/debit", new
                {
                    items = new[]
                    {
                        new
                        {
                            productCode = "P-CONCURRENT-001",
                            quantity = 1
                        }
                    }
                });
            });

        var debitTaskA = DebitAsync(clientA);
        var debitTaskB = DebitAsync(clientB);

        startSignal.SetResult(true);

        await Task.WhenAll(debitTaskA, debitTaskB);

        var responses = new[] { debitTaskA.Result, debitTaskB.Result };
        var successfulResponses = responses.Count(response => response.StatusCode == HttpStatusCode.OK);
        var conflictResponses = responses.Count(response => response.StatusCode == HttpStatusCode.Conflict);

        Assert.Equal(1, successfulResponses);
        Assert.Equal(1, conflictResponses);

        var conflictResponse = responses.Single(response => response.StatusCode == HttpStatusCode.Conflict);
        var errorBody = await conflictResponse.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(errorBody);
        Assert.Contains("Insufficient stock", errorBody.Message, StringComparison.OrdinalIgnoreCase);

        var productResponse = await clientA.GetAsync("/api/products/code/P-CONCURRENT-001");
        var productBody = await productResponse.Content.ReadFromJsonAsync<ProductResponse>();

        Assert.Equal(HttpStatusCode.OK, productResponse.StatusCode);
        Assert.NotNull(productBody);
        Assert.Equal(0, productBody.StockQuantity);
    }

    private sealed class ProductResponse
    {
        public int StockQuantity { get; set; }
    }

    private sealed class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
