using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EstoqueService.Infrastructure.Persistence.Context;

public class EstoqueDbContextFactory : IDesignTimeDbContextFactory<EstoqueDbContext>
{
    public EstoqueDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EstoqueDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=estoque_db;Username=postgres;Password=postgres",
            npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(EstoqueDbContext).Assembly.FullName));

        return new EstoqueDbContext(optionsBuilder.Options);
    }
}
