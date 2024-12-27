namespace Deevenue.Domain.Media;

public interface IOrphanTagsRepository
{
    IAsyncDisposable GetOrphanDisposableAsync();
}
