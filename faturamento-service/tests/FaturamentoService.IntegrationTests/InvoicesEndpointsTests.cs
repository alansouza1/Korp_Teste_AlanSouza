using System.Net;
using System.Net.Http.Json;
using FaturamentoService.Application.DTOs.Responses;
using FaturamentoService.Application.Exceptions;
using FaturamentoService.IntegrationTests.Infrastructure;
using Xunit;

namespace FaturamentoService.IntegrationTests;

public class InvoicesEndpointsTests
{
    [Fact]
    public async Task CreateInvoice_ShouldReturnCreated()
    {
        await using var factory = new FaturamentoApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/invoices", new { });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        Assert.NotNull(body);
        Assert.Equal("OPEN", body.Status);
        Assert.Empty(body.Items);
        Assert.True(body.SequentialNumber > 0);
    }

    [Fact]
    public async Task ListInvoices_ShouldReturnCreatedInvoices()
    {
        await using var factory = new FaturamentoApiFactory();
        using var client = factory.CreateClient();

        await CreateInvoiceAsync(client);
        await CreateInvoiceAsync(client);

        var response = await client.GetAsync("/api/invoices");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<InvoiceResponse>>();
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
        Assert.True(body[0].SequentialNumber > body[1].SequentialNumber);
    }

    [Fact]
    public async Task GetInvoiceById_ShouldReturnInvoice()
    {
        await using var factory = new FaturamentoApiFactory();
        using var client = factory.CreateClient();

        var createdInvoice = await CreateInvoiceAsync(client);

        var response = await client.GetAsync($"/api/invoices/{createdInvoice.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        Assert.NotNull(body);
        Assert.Equal(createdInvoice.Id, body.Id);
        Assert.Equal(createdInvoice.SequentialNumber, body.SequentialNumber);
    }

    [Fact]
    public async Task GetInvoiceBySequentialNumber_ShouldReturnInvoice()
    {
        await using var factory = new FaturamentoApiFactory();
        using var client = factory.CreateClient();

        var createdInvoice = await CreateInvoiceAsync(client);

        var response = await client.GetAsync($"/api/invoices/number/{createdInvoice.SequentialNumber}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        Assert.NotNull(body);
        Assert.Equal(createdInvoice.Id, body.Id);
        Assert.Equal(createdInvoice.SequentialNumber, body.SequentialNumber);
    }

    [Fact]
    public async Task AddItem_ToOpenInvoice_ShouldReturnUpdatedInvoice()
    {
        await using var factory = new FaturamentoApiFactory();
        using var client = factory.CreateClient();

        var invoice = await CreateInvoiceAsync(client);

        var response = await client.PostAsJsonAsync($"/api/invoices/{invoice.Id}/items", new
        {
            items = new[]
            {
                new
                {
                    productCode = "P001",
                    productDescription = "Produto Teste",
                    quantity = 2
                }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        Assert.NotNull(body);
        Assert.Single(body.Items);
        Assert.Equal("OPEN", body.Status);
        Assert.Equal("P001", body.Items[0].ProductCode);
    }

    [Fact]
    public async Task AddItem_ToClosedInvoice_ShouldReturnConflict()
    {
        await using var factory = new FaturamentoApiFactory();
        using var client = factory.CreateClient();

        var fakeStockClient = factory.GetFakeStockClient();
        fakeStockClient.ValidateHandler = (items, _) => Task.FromResult(new StockValidationResultDto
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

        var invoice = await CreateInvoiceWithItemAsync(client);
        var printResponse = await client.PostAsync($"/api/invoices/{invoice.Id}/print", null);
        Assert.Equal(HttpStatusCode.OK, printResponse.StatusCode);

        var addResponse = await client.PostAsJsonAsync($"/api/invoices/{invoice.Id}/items", new
        {
            items = new[]
            {
                new
                {
                    productCode = "P002",
                    productDescription = "Outro Produto",
                    quantity = 1
                }
            }
        });

        Assert.Equal(HttpStatusCode.Conflict, addResponse.StatusCode);
    }

    [Fact]
    public async Task PrintInvoice_Successfully_ShouldCloseInvoice()
    {
        await using var factory = new FaturamentoApiFactory();
        using var client = factory.CreateClient();

        var fakeStockClient = factory.GetFakeStockClient();
        fakeStockClient.ValidateHandler = (items, _) => Task.FromResult(new StockValidationResultDto
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
        fakeStockClient.DebitHandler = (_, _) => Task.CompletedTask;

        var invoice = await CreateInvoiceWithItemAsync(client);

        var response = await client.PostAsync($"/api/invoices/{invoice.Id}/print", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PrintInvoiceResponse>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.Equal("CLOSED", body.Invoice.Status);
        Assert.Equal(1, body.Invoice.PrintAttempts);
        Assert.Null(body.Invoice.LastPrintError);
        Assert.NotNull(body.Invoice.ClosedAt);
    }

    [Fact]
    public async Task PrintClosedInvoice_ShouldReturnConflict()
    {
        await using var factory = new FaturamentoApiFactory();
        using var client = factory.CreateClient();

        var fakeStockClient = factory.GetFakeStockClient();
        fakeStockClient.ValidateHandler = (items, _) => Task.FromResult(new StockValidationResultDto
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

        var invoice = await CreateInvoiceWithItemAsync(client);
        await client.PostAsync($"/api/invoices/{invoice.Id}/print", null);

        var secondPrint = await client.PostAsync($"/api/invoices/{invoice.Id}/print", null);

        Assert.Equal(HttpStatusCode.Conflict, secondPrint.StatusCode);
    }

    [Fact]
    public async Task PrintFailure_ShouldKeepInvoiceOpenAndPersistLastPrintError_ForInsufficientStock()
    {
        await using var factory = new FaturamentoApiFactory();
        using var client = factory.CreateClient();

        var fakeStockClient = factory.GetFakeStockClient();
        fakeStockClient.ValidateHandler = (items, _) => Task.FromResult(new StockValidationResultDto
        {
            IsValid = false,
            Items = items.Select(x => new StockItemAvailabilityDto
            {
                ProductCode = x.ProductCode,
                RequestedQuantity = x.Quantity,
                AvailableQuantity = 0,
                IsAvailable = false,
                Message = "Insufficient stock."
            }).ToList()
        });

        var invoice = await CreateInvoiceWithItemAsync(client);

        var printResponse = await client.PostAsync($"/api/invoices/{invoice.Id}/print", null);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, printResponse.StatusCode);

        var invoiceResponse = await client.GetAsync($"/api/invoices/{invoice.Id}");
        var invoiceBody = await invoiceResponse.Content.ReadFromJsonAsync<InvoiceResponse>();

        Assert.NotNull(invoiceBody);
        Assert.Equal("OPEN", invoiceBody.Status);
        Assert.Equal(1, invoiceBody.PrintAttempts);
        Assert.False(string.IsNullOrWhiteSpace(invoiceBody.LastPrintError));
        Assert.Contains("Insufficient stock", invoiceBody.LastPrintError, StringComparison.OrdinalIgnoreCase);
        Assert.Null(invoiceBody.ClosedAt);
    }

    [Fact]
    public async Task PrintFailure_ShouldKeepInvoiceOpenAndPersistLastPrintError_ForUnknownProduct()
    {
        await using var factory = new FaturamentoApiFactory();
        using var client = factory.CreateClient();

        var fakeStockClient = factory.GetFakeStockClient();
        fakeStockClient.ValidateHandler = (items, _) => Task.FromResult(new StockValidationResultDto
        {
            IsValid = false,
            Items = items.Select(x => new StockItemAvailabilityDto
            {
                ProductCode = x.ProductCode,
                RequestedQuantity = x.Quantity,
                AvailableQuantity = 0,
                IsAvailable = false,
                Message = "Product not found."
            }).ToList()
        });

        var invoice = await CreateInvoiceWithItemAsync(client, "INEXISTENTE");

        var printResponse = await client.PostAsync($"/api/invoices/{invoice.Id}/print", null);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, printResponse.StatusCode);

        var invoiceResponse = await client.GetAsync($"/api/invoices/{invoice.Id}");
        var invoiceBody = await invoiceResponse.Content.ReadFromJsonAsync<InvoiceResponse>();

        Assert.NotNull(invoiceBody);
        Assert.Equal("OPEN", invoiceBody.Status);
        Assert.Equal(1, invoiceBody.PrintAttempts);
        Assert.Contains("Product not found", invoiceBody.LastPrintError, StringComparison.OrdinalIgnoreCase);
        Assert.Null(invoiceBody.ClosedAt);
    }

    [Fact]
    public async Task PrintFailure_ShouldKeepInvoiceOpenAndPersistLastPrintError_ForErro500()
    {
        await using var factory = new FaturamentoApiFactory();
        using var client = factory.CreateClient();

        var fakeStockClient = factory.GetFakeStockClient();
        fakeStockClient.ValidateHandler = (items, _) =>
        {
            if (items.Any(x => string.Equals(x.ProductCode, "ERRO500", StringComparison.OrdinalIgnoreCase)))
            {
                throw new ExternalServiceException("Unable to process stock right now. Please try again later.");
            }

            return Task.FromResult(new StockValidationResultDto { IsValid = true });
        };

        var invoice = await CreateInvoiceWithItemAsync(client, "ERRO500");

        var printResponse = await client.PostAsync($"/api/invoices/{invoice.Id}/print", null);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, printResponse.StatusCode);

        var invoiceResponse = await client.GetAsync($"/api/invoices/{invoice.Id}");
        var invoiceBody = await invoiceResponse.Content.ReadFromJsonAsync<InvoiceResponse>();

        Assert.NotNull(invoiceBody);
        Assert.Equal("OPEN", invoiceBody.Status);
        Assert.Equal(1, invoiceBody.PrintAttempts);
        Assert.False(string.IsNullOrWhiteSpace(invoiceBody.LastPrintError));
        Assert.Contains("Unable to process stock right now", invoiceBody.LastPrintError, StringComparison.OrdinalIgnoreCase);
        Assert.Null(invoiceBody.ClosedAt);
    }

    private static async Task<InvoiceResponse> CreateInvoiceAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/invoices", new { });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<InvoiceResponse>())!;
    }

    private static async Task<InvoiceResponse> CreateInvoiceWithItemAsync(HttpClient client, string productCode = "P001")
    {
        var invoice = await CreateInvoiceAsync(client);

        var addItemsResponse = await client.PostAsJsonAsync($"/api/invoices/{invoice.Id}/items", new
        {
            items = new[]
            {
                new
                {
                    productCode,
                    productDescription = "Produto Teste",
                    quantity = 2
                }
            }
        });

        addItemsResponse.EnsureSuccessStatusCode();
        return (await addItemsResponse.Content.ReadFromJsonAsync<InvoiceResponse>())!;
    }

    private sealed class InvoiceResponse
    {
        public Guid Id { get; set; }
        public int SequentialNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ClosedAt { get; set; }
        public int PrintAttempts { get; set; }
        public string? LastPrintError { get; set; }
        public List<InvoiceItemResponse> Items { get; set; } = [];
    }

    private sealed class InvoiceItemResponse
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    private sealed class PrintInvoiceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public InvoiceResponse Invoice { get; set; } = new();
    }
}
