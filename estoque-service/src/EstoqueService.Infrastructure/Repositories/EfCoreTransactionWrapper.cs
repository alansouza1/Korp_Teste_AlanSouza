using Microsoft.EntityFrameworkCore.Storage;

using EstoqueService.Application.Interfaces;

namespace EstoqueService.Infrastructure.Repositories;

public sealed class EfCoreTransactionWrapper : IAppTransaction
{
    private readonly IDbContextTransaction _transaction;

    public EfCoreTransactionWrapper(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return _transaction.CommitAsync(cancellationToken);
    }

    public void Dispose()
    {
        _transaction.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _transaction.DisposeAsync();
    }
}
