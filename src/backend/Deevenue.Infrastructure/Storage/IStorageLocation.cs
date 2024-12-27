using Deevenue.Domain;

namespace Deevenue.Infrastructure.Storage;

public interface IStorageLocation
{
    IBucket Bucket { get; }
    string FileName { get; }
}
