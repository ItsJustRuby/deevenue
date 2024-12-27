namespace Deevenue.Domain.Media;

public interface IMediumStorage
{
    Task<MediumData?> GetAsync(Guid id);
    Task RemoveAsync(Guid id);
    Task StoreAsync(Guid id, string ContentType, long Size, Stream ReadStream);
}
