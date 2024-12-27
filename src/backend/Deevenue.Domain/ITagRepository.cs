namespace Deevenue.Domain;

public interface ITagRepository
{
    Task EnsureExistAsync(IReadOnlySet<string> names);
    Task<TagViewModel?> FindAsync(string name);
    Task<AllTagsViewModel> GetAllAsync();

    Task<bool> AddAliasAsync(string name, string alias);
    Task<bool> AddImplicationAsync(string implying, string implied);
    Task<bool> RemoveAliasAsync(string name, string alias);
    Task<bool> RemoveImplicationAsync(string implying, string implied);
    Task<bool> RenameAsync(string currentName, string newName);
    Task<bool> SetRatingAsync(string tagName, Rating rating);
}

public sealed record AllTagsViewModel(IEnumerable<TagViewModel> Tags);

public sealed record TagViewModel(
    string Name,
    int MediaCount,
    Rating Rating,
    IEnumerable<string> Aliases,
    IEnumerable<string> ImplyingThis,
    IEnumerable<string> ImpliedByThis
);
