using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FaturamentoService.Infrastructure.Persistence.Context;

public class FaturamentoDbContextFactory : IDesignTimeDbContextFactory<FaturamentoDbContext>
{
    public FaturamentoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FaturamentoDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=faturamento_db;Username=postgres;Password=postgres",
            npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(FaturamentoDbContext).Assembly.FullName));

        return new FaturamentoDbContext(optionsBuilder.Options);
    }
}
