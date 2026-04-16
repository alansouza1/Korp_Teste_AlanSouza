using FaturamentoService.Api.Extensions;
using FaturamentoService.Api.Middleware;
using FaturamentoService.Infrastructure;
using FaturamentoService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogConfiguration();

builder.Services.AddApiServices();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FaturamentoDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");
    await InitializeDatabaseAsync(dbContext, logger);
}

app.MapControllers();

await app.RunAsync();

static async Task InitializeDatabaseAsync(
    FaturamentoDbContext dbContext,
    Microsoft.Extensions.Logging.ILogger logger,
    CancellationToken cancellationToken = default)
{
    const int maxAttempts = 10;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            if (dbContext.Database.IsRelational())
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
            }
            else
            {
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            }

            logger.LogInformation("Database initialization completed on attempt {Attempt}", attempt);
            return;
        }
        catch (Exception exception) when (attempt < maxAttempts)
        {
            logger.LogWarning(exception, "Database initialization failed on attempt {Attempt}. Retrying...", attempt);
            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        }
    }

    if (dbContext.Database.IsRelational())
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}

public partial class Program
{
}
