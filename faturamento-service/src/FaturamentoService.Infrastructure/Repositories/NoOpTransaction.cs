using FaturamentoService.Application.Interfaces;

namespace FaturamentoService.Infrastructure.Repositories;

public sealed class NoOpTransaction : IAppTransaction
{
    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
