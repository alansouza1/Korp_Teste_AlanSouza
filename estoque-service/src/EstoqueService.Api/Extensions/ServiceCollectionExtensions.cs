using EstoqueService.Application.DTOs.Requests;
using EstoqueService.Application.Interfaces;
using EstoqueService.Application.Services;
using EstoqueService.Application.Validators;
using FluentValidation;
using Microsoft.OpenApi.Models;
using Serilog;

namespace EstoqueService.Api.Extensions;

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
                Title = "Estoque Service API",
                Version = "v1",
                Description = "Microservice responsible for product and stock management."
            });
        });

        services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
        services.AddScoped<IProductDescriptionSuggestionService, DeterministicProductDescriptionSuggestionService>();
        services.AddScoped<IProductService, ProductService>();

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
