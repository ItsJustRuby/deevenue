using Deevenue.Domain;

namespace Deevenue.Infrastructure.Db;

internal interface IInternalTagRepository : ITagRepository
{
    Task<Tag> EnsureExistsAsync(string name);
}
