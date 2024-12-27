namespace Deevenue.Domain;

public interface IBucketStorage
{
    Task CreateAsync(IBucket bucket);
    Task RemoveAsync(IBucket bucket);
}
