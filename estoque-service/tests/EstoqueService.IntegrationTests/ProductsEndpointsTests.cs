using System.Net;
using System.Net.Http.Json;
using EstoqueService.IntegrationTests.Infrastructure;
using Xunit;

namespace EstoqueService.IntegrationTests;

public class ProductsEndpointsTests
{
    [Fact]
    public async Task CreateProduct_ShouldReturnCreated()
    {
        await using var factory = new EstoqueApiFactory();
        using var client = factory.CreateClient();

        var request = new
        {
            code = "P001",
            description = "Produto Teste",
            stockQuantity = 10
        };

        var response = await client.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(body);
        Assert.Equal("P001", body.Code);
        Assert.Equal("Produto Teste", body.Description);
        Assert.Equal(10, body.StockQuantity);
        Assert.NotEqual(Guid.Empty, body.Id);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateCode_ShouldReturnConflict()
    {
        await using var factory = new EstoqueApiFactory();
        using var client = factory.CreateClient();

        var request = new
        {
            code = "P001",
            description = "Produto Teste",
            stockQuantity = 10
        };

        await client.PostAsJsonAsync("/api/products", request);
        var duplicateResponse = await client.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var error = await duplicateResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains("already exists", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateStock_ShouldReturnAvailabilityPerItem()
    {
        await using var factory = new EstoqueApiFactory();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/products", new
        {
            code = "P001",
            description = "Produto Teste",
            stockQuantity = 10
        });

        var response = await client.PostAsJsonAsync("/api/products/stock/validate", new
        {
            items = new[]
            {
                new { productCode = "P001", quantity = 5 },
                new { productCode = "P999", quantity = 1 }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ValidateStockResponse>();
        Assert.NotNull(body);
        Assert.False(body.IsValid);
        Assert.Contains(body.Items, x => x.ProductCode == "P001" && x.IsAvailable);
        Assert.Contains(body.Items, x => x.ProductCode == "P999" && !x.IsAvailable);
    }

    [Fact]
    public async Task DebitStock_ShouldReduceStockAndReturnSuccess()
    {
        await using var factory = new EstoqueApiFactory();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/products", new
        {
            code = "P001",
            description = "Produto Teste",
            stockQuantity = 10
        });

        var debitResponse = await client.PostAsJsonAsync("/api/products/stock/debit", new
        {
            items = new[]
            {
                new { productCode = "P001", quantity = 4 }
            }
        });

        Assert.Equal(HttpStatusCode.OK, debitResponse.StatusCode);

        var debitBody = await debitResponse.Content.ReadFromJsonAsync<DebitStockResponse>();
        Assert.NotNull(debitBody);
        Assert.True(debitBody.Success);
        Assert.Contains(debitBody.UpdatedProducts, x => x.Code == "P001" && x.StockQuantity == 6);

        var productResponse = await client.GetAsync("/api/products/code/P001");
        var productBody = await productResponse.Content.ReadFromJsonAsync<ProductResponse>();

        Assert.Equal(HttpStatusCode.OK, productResponse.StatusCode);
        Assert.NotNull(productBody);
        Assert.Equal(6, productBody.StockQuantity);
    }

    [Fact]
    public async Task ValidateStock_WithErro500_ShouldReturnServiceUnavailable()
    {
        await using var factory = new EstoqueApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/products/stock/validate", new
        {
            items = new[]
            {
                new { productCode = "ERRO500", quantity = 1 }
            }
        });

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("ESTOQUE_SIMULATED_FAILURE", error.ErrorCode);
    }

    private sealed class ProductResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
    }

    private sealed class ValidateStockResponse
    {
        public bool IsValid { get; set; }
        public List<ValidateStockItemResponse> Items { get; set; } = [];
    }

    private sealed class ValidateStockItemResponse
    {
        public string ProductCode { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }

    private sealed class DebitStockResponse
    {
        public bool Success { get; set; }
        public List<ProductResponse> UpdatedProducts { get; set; } = [];
    }

    private sealed class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
    }
}
