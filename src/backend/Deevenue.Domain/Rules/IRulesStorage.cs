namespace Deevenue.Domain.Rules;

public interface IRulesStorage
{
    Task StoreAsync(Stream stream);
    Task<Stream?> StreamAsync();
}
