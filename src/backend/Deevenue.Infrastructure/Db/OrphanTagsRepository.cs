using Deevenue.Domain.Media;
using Microsoft.EntityFrameworkCore;

namespace Deevenue.Infrastructure.Db;

internal class OrphanTagsRepository(DeevenueContext dbContext) : IOrphanTagsRepository
{
    public IAsyncDisposable GetOrphanDisposableAsync() => new OrphanDisposable(this);

    private async Task DeleteOrphansAsync()
    {
        // This is not very comforting to read, but easier to implement a check for connected
        // components in the implications graph.
        while (true)
        {
            var orphans = await dbContext
                .Tags
                .Where(t =>
                    t.Media.Count() == 0 &&
                    t.AbsentMedia.Count() == 0 &&
                    t.ImplyingThis.Count() == 0 &&
                    t.ImpliedByThis.Count() == 0)
                .ToListAsync();

            if (orphans.Count == 0)
                return;

            dbContext.Tags.RemoveRange(orphans);
            await dbContext.SaveChangesAsync();
        }
    }

    private class OrphanDisposable(OrphanTagsRepository repository) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync() => await repository.DeleteOrphansAsync();
    }
}
