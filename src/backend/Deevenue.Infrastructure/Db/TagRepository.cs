using Deevenue.Domain;
using Deevenue.Domain.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Deevenue.Infrastructure.Db;

internal class TagRepository(
    IOrphanTagsRepository orphanTagsRepository,
    DeevenueContext dbContext,
    ILogger<TagRepository> logger) : IInternalTagRepository
{
    public async Task<bool> AddAliasAsync(string name, string alias)
    {
        var tag = await dbContext
            .Tags
            .SingleOrDefaultAsync(t => t.Name == name);

        if (tag == null)
            return false;

        if (await IsNameAlreadyTakenAsync(alias))
            return false;

        tag.Aliases.Add(alias);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddImplicationAsync(string implying, string implied)
    {
        var implyingTag = await TryGetAsync(implying);

        if (implyingTag == null)
            return false;

        var maybeCurrentImpliedTag = await TryGetAsync(implied);

        if (maybeCurrentImpliedTag != null)
        {
            var wouldCauseCycle = await WouldCauseCycleAsync(implyingTag, maybeCurrentImpliedTag);
            if (wouldCauseCycle)
                return false;
        }

        var impliedTag = await EnsureExistsAsync(implied);
        implyingTag.ImpliedByThis.Add(impliedTag);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task EnsureExistAsync(IReadOnlySet<string> names)
    {
        if (names.Count == 0)
            return;

        var currentNames = await dbContext
            .Tags
            .Where(t => names.Contains(t.Name) || t.Aliases.Any(a => names.Contains(a)))
            .Select(t => t.Name)
            .ToHashSetAsync();

        var namesToInsert = names.Except(currentNames).ToHashSet();
        foreach (var name in namesToInsert)
        {
            var toInsert = new Tag { Name = name, Rating = Rating.Unknown };
            await dbContext.Tags.AddAsync(toInsert);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<Tag> EnsureExistsAsync(string name)
    {
        var maybeCurrent = await TryGetAsync(name);
        if (maybeCurrent != null)
            return maybeCurrent;

        var brandNew = new Tag { Name = name, Rating = Rating.Unknown };
        await dbContext.Tags.AddAsync(brandNew);
        return brandNew;
    }

    public async Task<TagViewModel?> FindAsync(string name)
    {
        var maybeTag = await dbContext.Tags
            .Include(t => t.ImpliedByThis)
            .Include(t => t.ImplyingThis)
            .Where(t => t.Name == name || t.Aliases.Contains(name))
            .Select(t => Tuple.Create(t, t.Media.Count()))
            .SingleOrDefaultAsync();

        if (maybeTag == null)
            return null;

        return new TagViewModel(
            maybeTag.Item1.Name,
            maybeTag.Item2,
            maybeTag.Item1.Rating,
            maybeTag.Item1.Aliases,
            maybeTag.Item1.ImplyingThis.Select(t => t.Name).ToList(),
            maybeTag.Item1.ImpliedByThis.Select(t => t.Name).ToList()
        );
    }

    public async Task<AllTagsViewModel> GetAllAsync()
    {
        var tags = await dbContext.Tags
            .Include(t => t.ImpliedByThis)
            .Include(t => t.ImplyingThis)
            .OrderByDescending(t => t.Media.Count())
            .ThenByDescending(t => t.Name)
            .ToListAsync();

        var mediaCountByName = await dbContext
            .Tags
            .Select(t => Tuple.Create(t.Name, t.Media.Count()))
            .ToDictionaryAsync(t => t.Item1, t => t.Item2);

        return new AllTagsViewModel(tags.Select(t =>
        {
            return new TagViewModel(
                t.Name,
                mediaCountByName[t.Name],
                t.Rating,
                t.Aliases,
                t.ImplyingThis.Select(t => t.Name).ToList(),
                t.ImpliedByThis.Select(t => t.Name).ToList()
            );
        }));
    }

    public async Task<bool> RemoveAliasAsync(string name, string alias)
    {
        var tag = await dbContext
            .Tags
            .SingleOrDefaultAsync(t => t.Name == name);

        if (tag == null)
            return false;

        tag.Aliases.Remove(alias);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveImplicationAsync(string implying, string implied)
    {
        var tag = await dbContext
        .Tags
        .Include(t => t.ImpliedByThis)
        .SingleOrDefaultAsync(t => t.Name == implying);

        if (tag == null)
            return false;

        await using var orphanDisposable = orphanTagsRepository.GetOrphanDisposableAsync();

        tag.ImpliedByThis = tag.ImpliedByThis.Where(t => t.Name != implied).ToHashSet();
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RenameAsync(string currentName, string newName)
    {
        var tag = await dbContext
            .Tags
            .SingleOrDefaultAsync(t => t.Name == currentName);

        if (tag == null)
            return false;

        if (await IsNameAlreadyTakenAsync(newName))
            return false;

        tag.Name = newName;
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetRatingAsync(string tagName, Rating rating)
    {
        var tag = await dbContext
            .Tags
            .SingleOrDefaultAsync(t => t.Name == tagName);

        if (tag == null)
            return false;

        tag.Rating = rating;
        await dbContext.SaveChangesAsync();
        return true;
    }

    private async Task<bool> IsNameAlreadyTakenAsync(string name)
    {
        return await dbContext.Tags
            .AnyAsync(t => t.Name == name || t.Aliases.Contains(name));
    }

    private async Task<Tag?> TryGetAsync(string name)
    {
        return await dbContext.Tags
            .Where(t => t.Name == name || t.Aliases.Contains(name))
            .SingleOrDefaultAsync();
    }


    // Given the amount of data we are talking about, just loading the entire Implications
    // table is absolutely fine. You do not need to write a fancy recursive SQL CTE,
    // as funny as it would be.
    private async Task<bool> WouldCauseCycleAsync(Tag implyingTag, Tag maybeCurrentImpliedTag)
    {
        var currentImplicationEntities = await dbContext.TagImplications.ToHashSetAsync();

        var currentImplicationEdgesByEndpoint = currentImplicationEntities
            .ToLookup(ti => ti.ImpliedTagId, ti => ti.ImplyingTagId);

        // Walk all implication edge "arrows" backwards from the current "implying"
        // and if you reach "implied", you have a cycle.
        var hasCycle = false;
        var potentialNodesInCycle = new Queue<Guid>();
        potentialNodesInCycle.Enqueue(implyingTag.Id);
        while (potentialNodesInCycle.Count > 0 && !hasCycle)
        {
            var candidate = potentialNodesInCycle.Dequeue();
            hasCycle = candidate == maybeCurrentImpliedTag.Id;

            foreach (var incomingEdge in currentImplicationEdgesByEndpoint[candidate])
                potentialNodesInCycle.Enqueue(incomingEdge);
        }

        if (hasCycle)
        {
            logger.LogDebug("Tried to insert an implication cycle");
            return false;
        }

        return true;
    }
}
