using EstoqueService.Application.Interfaces;

namespace EstoqueService.Infrastructure.Repositories;

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
