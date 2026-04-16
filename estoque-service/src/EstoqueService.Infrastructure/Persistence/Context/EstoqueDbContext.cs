using EstoqueService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Infrastructure.Persistence.Context;

public class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EstoqueDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
