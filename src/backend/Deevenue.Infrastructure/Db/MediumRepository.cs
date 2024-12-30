using Deevenue.Domain;
using Deevenue.Domain.Media;
using Microsoft.EntityFrameworkCore;

namespace Deevenue.Infrastructure.Db;

internal class MediumRepository(DeevenueContext dbContext) : IMediumRepository
{
    public async Task<bool> ExistsAsync(Guid id)
    {
        return (await dbContext.Media.SingleOrDefaultAsync(m => m.Id == id)) != null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var maybeMedium = await dbContext
            .Media
            .SingleOrDefaultAsync(m => m.Id == id);

        if (maybeMedium == null)
            return false;

        dbContext.Remove(maybeMedium);
        dbContext.SaveChanges();
        return true;
    }

    public Task<Guid?> FindByHashAsync(string hash)
    {
        return dbContext.MediumHashes
            .FirstOrDefaultAsync(m => m.Hash == hash)
            .ContinueWith(m => m.Result?.MediumId);
    }

    public async Task<IReadOnlySet<SmallMediumDocument>> GetAllSearchableAsync(bool isSfw)
    {
        var baseQuery = dbContext.Media.AsQueryable();

        if (isSfw)
            baseQuery = baseQuery.Where(m => m.Rating == Rating.Safe);

        var all = await baseQuery
            .Include(m => m.Tags)
            .Include(m => m.AbsentTags)
            .ToListAsync();

        return await GetSearchableAsync(all);
    }

    public async Task<SmallMediumDocument?> GetSearchableAsync(Guid id, bool isSfw)
    {
        var baseQuery = dbContext.Media
            .AsQueryable();

        if (isSfw)
            baseQuery = baseQuery.Where(m => m.Rating == Rating.Safe);

        var single = await baseQuery
            .Include(m => m.Tags)
            .Include(m => m.AbsentTags)
            .SingleOrDefaultAsync(m => m.Id == id);

        if (single == null)
            return null;

        return (await GetSearchableAsync([single])).Single();
    }

    public async Task<PaginateAllResult> PaginateAllAsync(PaginationParameters pagination, bool isSfw)
    {
        var baseQuery = dbContext.Media.AsQueryable();

        if (isSfw)
            baseQuery = baseQuery.Where(m => m.Rating == Rating.Safe);

        var totalCount = await baseQuery.CountAsync();
        var pageCount = (int)Math.Ceiling((decimal)totalCount / pagination.PageSize);

        var page = await baseQuery
            .OrderByDescending(m => m.InsertedAt)
            .Paginate(pagination)
            .Select(m => new PaginateAllDataSet(m.Id, m.ContentType))
            .ToListAsync();

        return new PaginateAllResult(pageCount, page);
    }

    public async Task<Guid> PutAsync(PutAsyncArgs args)
    {

        var tagsToLink = await dbContext.Tags
            .Where(t => args.TagNames.Contains(t.Name) || t.Aliases.Any(a => args.TagNames.Contains(a)))
            .ToHashSetAsync();

        var medium = new Medium
        {
            FileSize = args.Size,
            ContentType = args.ContentType,
            Width = args.Width ?? 0,
            Height = args.Height ?? 0,
            Rating = args.Rating,
            Tags = tagsToLink,
            AbsentTags = new HashSet<Tag>(),
            InsertedAt = DateTime.UtcNow
        };

        var hash = new MediumHash
        {
            Hash = args.Hash,
            Medium = medium
        };

        await dbContext.Media.AddAsync(medium);

        await dbContext.MediumHashes.AddAsync(hash);
        await dbContext.SaveChangesAsync();

        return medium.Id;
    }

    public async Task UpdateContentMetadataAsync(Guid id, string contentType, string hash)
    {
        var maybeMedium = await dbContext.Media.SingleOrDefaultAsync(m => m.Id == id);
        if (maybeMedium == null)
            return;

        maybeMedium.ContentType = contentType;

        var hashEntity = new MediumHash
        {
            Hash = hash,
            Medium = maybeMedium
        };

        await dbContext.MediumHashes.AddAsync(hashEntity);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> SetRatingAsync(Guid id, Rating rating)
    {
        var maybeMedium = await dbContext.Media.SingleOrDefaultAsync(m => m.Id == id);
        if (maybeMedium == null)
            return false;

        maybeMedium.Rating = rating;
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<MediumDataSet?> TryGetAsync(Guid id)
    {
        var maybeMedium = await dbContext.Media
            .Include(m => m.Tags)
            .Include(m => m.AbsentTags)
            .Include(m => m.ThumbnailSheets)
            .SingleOrDefaultAsync(m => m.Id == id);

        if (maybeMedium == null)
            return null;

        return new MediumDataSet(
            id,
            maybeMedium.ContentType,
            maybeMedium.Tags.Select(t => t.Name).ToList(),
            maybeMedium.AbsentTags.Select(t => t.Name).ToList(),
            maybeMedium.ThumbnailSheets.Select(ts => ts.Id).ToList(),
            maybeMedium.Rating);
    }

    public async Task UpdateDimensionsAsync(Guid id, MediumDimensions dimensions)
    {
        var maybeMedium = await dbContext.Media
            .SingleOrDefaultAsync(m => m.Id == id);

        if (maybeMedium == null) return;

        maybeMedium.Width = dimensions.Width;
        maybeMedium.Height = dimensions.Height;
        await dbContext.SaveChangesAsync();
    }

    private async Task<HashSet<SmallMediumDocument>> GetSearchableAsync(IEnumerable<Medium> media)
    {
        var tagNamesByMediumId = await GetSearchableTagNamesAsync(media.Select(m => m.Id).ToHashSet());

        return media.Select(m => new SmallMediumDocument(
            m.Id,
            m.ContentType,
            InnateTags: m.Tags
                .Select(t => new HashSet<string>(t.Aliases) { t.Name })
                .SelectMany(s => s)
                .ToHashSet(),
            SearchableTags: tagNamesByMediumId.Searchable[m.Id].ToHashSet(),
            AbsentTags: tagNamesByMediumId.Absent[m.Id].ToHashSet(),
            m.Rating,
            m.FileSize,
            m.InsertedAt,
            m.Width,
            m.Height
        )).ToHashSet();
    }

    private async Task<TagNamesByMediumId>
        GetSearchableTagNamesAsync(HashSet<Guid> mediumIds)
    {
        var allTagImplications = dbContext.TagImplications.ToLookup(ti => ti.ImplyingTagId, ti => ti.ImpliedTagId);

        var relevantMediumTags = await dbContext.MediumTags
            .Where(mt => mediumIds.Contains(mt.MediumId))
            .Select(t => (ITagLinkedToMedium)t)
            .ToHashSetAsync();

        var relevantMediumTagAbsences = await dbContext.MediumTagAbsences
            .Where(mta => mediumIds.Contains(mta.MediumId))
            .Select(t => (ITagLinkedToMedium)t)
            .ToHashSetAsync();

        return new TagNamesByMediumId(
            await Foo(mediumIds, allTagImplications, relevantMediumTags),
            await Foo(mediumIds, allTagImplications, relevantMediumTagAbsences)
        );
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlySet<string>>> Foo(
        HashSet<Guid> mediumIds, ILookup<Guid, Guid> allTagImplications, IReadOnlySet<ITagLinkedToMedium> links)
    {
        var innateTagIdsByMediumId = links.ToLookup(mt => mt.MediumId, mt => mt.TagId);

        var reachableTagIdsByMediumId = new Dictionary<Guid, IReadOnlySet<Guid>>();
        foreach (var grouping in innateTagIdsByMediumId)
        {
            var mediumId = grouping.Key;
            var innateTagIds = grouping.ToHashSet();

            var reachableTagIds = new HashSet<Guid>(innateTagIds);
            var candidates = new Queue<Guid>(innateTagIds);
            while (candidates.Count > 0)
            {
                var candidate = candidates.Dequeue();
                var newCandidates = allTagImplications[candidate].ToHashSet();
                foreach (var c in newCandidates)
                {
                    reachableTagIds.Add(c);
                    candidates.Enqueue(c);
                }
            }
            reachableTagIdsByMediumId[mediumId] = reachableTagIds;
        }

        var relevantTagIds = reachableTagIdsByMediumId.Values.SelectMany(v => v);

        var tagNamesAndAliasesById = await dbContext
            .Tags
            .Where(t => relevantTagIds.Contains(t.Id))
            .Select(t => Tuple.Create(t.Id, t.Name, t.Aliases))
            .ToDictionaryAsync(t => t.Item1, t => new HashSet<string>(t.Item3) { t.Item2 });

        var result = new Dictionary<Guid, IReadOnlySet<string>>();
        foreach (var mediumId in mediumIds)
        {
            if (reachableTagIdsByMediumId.TryGetValue(mediumId, out IReadOnlySet<Guid>? tagIds))
                result[mediumId] = tagIds.SelectMany(tagId => tagNamesAndAliasesById[tagId])
                    .ToHashSet();
            else
                result[mediumId] = new HashSet<string>();
        }

        return result;
    }

    private record TagNamesByMediumId(
        IReadOnlyDictionary<Guid, IReadOnlySet<string>> Searchable,
        IReadOnlyDictionary<Guid, IReadOnlySet<string>> Absent
    );
}
