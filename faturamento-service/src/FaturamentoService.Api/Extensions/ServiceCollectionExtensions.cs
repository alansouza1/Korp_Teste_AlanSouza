using FaturamentoService.Application.DTOs.Requests;
using FaturamentoService.Application.Interfaces;
using FaturamentoService.Application.Services;
using FaturamentoService.Application.Validators;
using FluentValidation;
using Microsoft.OpenApi.Models;
using Serilog;

namespace FaturamentoService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Faturamento Service API",
                Version = "v1",
                Description = "Microservice responsible for invoice management and stock integration."
            });
        });

        services.AddValidatorsFromAssemblyContaining<CreateInvoiceRequestValidator>();
        services.AddScoped<IInvoiceService, InvoiceService>();

        return services;
    }

    public static WebApplicationBuilder AddSerilogConfiguration(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console();
        });

        return builder;
    }
}
