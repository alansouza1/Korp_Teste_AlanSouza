namespace EstoqueService.Application.Interfaces;

public interface IAppTransaction : IDisposable, IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
