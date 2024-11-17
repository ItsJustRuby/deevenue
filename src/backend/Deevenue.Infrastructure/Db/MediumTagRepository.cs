using Deevenue.Domain.Media;
using Microsoft.EntityFrameworkCore;

namespace Deevenue.Infrastructure.Db;

internal class MediumTagRepository(
    DeevenueContext dbContext,
    IInternalTagRepository tagRepository) : IMediumTagRepository
{
    public async Task LinkAbsentAsync(Guid mediumId, string tagName)
    {
        var maybeMedium = await dbContext
            .Media
            .Include(m => m.AbsentTags)
            .SingleOrDefaultAsync(m => m.Id == mediumId);

        if (maybeMedium == null)
            return;

        var tag = await tagRepository.EnsureExistsAsync(tagName);
        maybeMedium.AbsentTags.Add(tag);
        await dbContext.SaveChangesAsync();
    }

    public async Task LinkAsync(Guid mediumId, string tagName)
    {
        var maybeMedium = await dbContext
            .Media
            .Include(m => m.Tags)
            .SingleOrDefaultAsync(m => m.Id == mediumId);

        if (maybeMedium == null)
            return;

        var tag = await tagRepository.EnsureExistsAsync(tagName);
        maybeMedium.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
    }

    public async Task UnlinkAbsentAsync(Guid mediumId, string tagName)
    {
        var maybeMedium = await dbContext
            .Media
            .Include(m => m.AbsentTags)
            .SingleOrDefaultAsync(m => m.Id == mediumId);

        if (maybeMedium == null)
            return;

        maybeMedium.AbsentTags = maybeMedium.AbsentTags.Where(t => t.Name != tagName).ToHashSet();
        await dbContext.SaveChangesAsync();
    }

    public async Task UnlinkAsync(Guid mediumId, string tagName)
    {
        var maybeMedium = await dbContext
            .Media
            .Include(m => m.Tags)
            .SingleOrDefaultAsync(m => m.Id == mediumId);

        if (maybeMedium == null)
            return;

        maybeMedium.Tags = maybeMedium.Tags.Where(t => t.Name != tagName).ToHashSet();
        await dbContext.SaveChangesAsync();
    }
}
