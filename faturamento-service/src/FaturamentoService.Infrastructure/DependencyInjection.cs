using FaturamentoService.Application.Interfaces;
using FaturamentoService.Infrastructure.Clients;
using FaturamentoService.Infrastructure.Persistence.Context;
using FaturamentoService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System.Net;

namespace FaturamentoService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FaturamentoDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(FaturamentoDbContext).Assembly.FullName)));

        services.Configure<StockServiceOptions>(configuration.GetSection(StockServiceOptions.SectionName));

        var stockServiceBaseUrl =
            configuration.GetSection(StockServiceOptions.SectionName).GetValue<string>("BaseUrl")
            ?? configuration.GetSection("Services").GetValue<string>("EstoqueBaseUrl")
            ?? throw new InvalidOperationException("Stock service base URL is not configured.");

        services.AddHttpClient<IStockServiceClient, StockServiceClient>(client =>
            {
                client.BaseAddress = new Uri(stockServiceBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddPolicyHandler(Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .OrResult(response => (int)response.StatusCode >= 500 && response.StatusCode != HttpStatusCode.ServiceUnavailable)
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(200 * attempt)));

        services.AddScoped<IInvoiceRepository, InvoiceRepository>();

        return services;
    }
}
