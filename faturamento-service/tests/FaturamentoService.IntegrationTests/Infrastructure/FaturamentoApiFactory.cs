using FaturamentoService.Application.Interfaces;
using FaturamentoService.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FaturamentoService.IntegrationTests.Infrastructure;

public class FaturamentoApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"faturamento-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Services:EstoqueBaseUrl", "http://fake-stock-service");
        builder.UseSetting("StockService:BaseUrl", "http://fake-stock-service");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Services:EstoqueBaseUrl"] = "http://fake-stock-service",
                ["StockService:BaseUrl"] = "http://fake-stock-service"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<FaturamentoDbContext>));
            services.RemoveAll(typeof(FaturamentoDbContext));
            services.RemoveAll(typeof(IStockServiceClient));
            services.RemoveAll(typeof(FakeStockServiceClient));

            services.AddDbContext<FaturamentoDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.AddSingleton<FakeStockServiceClient>();
            services.AddSingleton<IStockServiceClient>(provider => provider.GetRequiredService<FakeStockServiceClient>());
        });
    }

    public FakeStockServiceClient GetFakeStockClient()
    {
        return Services.GetRequiredService<FakeStockServiceClient>();
    }
}
