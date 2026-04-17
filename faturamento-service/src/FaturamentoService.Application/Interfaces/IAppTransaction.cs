namespace FaturamentoService.Application.Interfaces;

public interface IAppTransaction : IAsyncDisposable, IDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
