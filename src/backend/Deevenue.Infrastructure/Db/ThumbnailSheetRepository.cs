using Deevenue.Domain.Thumbnails;
using Microsoft.EntityFrameworkCore;

namespace Deevenue.Infrastructure.Db;

internal class ThumbnailSheetRepository(DeevenueContext dbContext) : IThumbnailSheetRepository
{
    public async Task CompleteAsync(Guid id)
    {
        var sheet = await dbContext
            .ThumbnailSheets
            .SingleOrDefaultAsync(ts => ts.Id == id);

        if (sheet == null)
            return;

        sheet.CompletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
    }

    public async Task<Guid> CreateAsync(Guid mediumId, int count)
    {
        var medium = await dbContext.Media.SingleAsync(m => m.Id == mediumId);

        var sheet = new ThumbnailSheet
        {
            Medium = medium,
            ThumbnailCount = count,
            InsertedAt = DateTime.UtcNow,
        };

        dbContext.ThumbnailSheets.Add(sheet);
        await dbContext.SaveChangesAsync();
        return sheet.Id;
    }

    public async Task<IReadOnlySet<Guid>> GetAllIncompleteAsync()
    {
        return await dbContext
            .ThumbnailSheets
            .Where(s => s.CompletedAt == null)
            .Select(s => s.Id)
            .ToHashSetAsync();
    }

    public async Task<ThumbnailSheetViewModel?> GetAsync(Guid id)
    {
        var sheet = await dbContext
            .ThumbnailSheets
            .SingleOrDefaultAsync(ts => ts.Id == id);

        if (sheet == null)
            return null;

        return new ThumbnailSheetViewModel(
            sheet.Id,
            sheet.MediumId,
            sheet.ThumbnailCount,
            IsComplete: sheet.CompletedAt != null);
    }

    public async Task RemoveAsync(Guid id)
    {
        var sheet = await dbContext
            .ThumbnailSheets
            .SingleOrDefaultAsync(ts => ts.Id == id);

        if (sheet == null)
            return;

        dbContext.Remove(sheet);
        await dbContext.SaveChangesAsync();
    }
}
