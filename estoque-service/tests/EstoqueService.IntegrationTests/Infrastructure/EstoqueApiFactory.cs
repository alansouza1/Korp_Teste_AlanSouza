using EstoqueService.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EstoqueService.IntegrationTests.Infrastructure;

public class EstoqueApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"estoque-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<EstoqueDbContext>));
            services.RemoveAll(typeof(EstoqueDbContext));

            services.AddDbContext<EstoqueDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
