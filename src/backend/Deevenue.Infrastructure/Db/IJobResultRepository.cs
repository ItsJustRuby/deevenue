using Microsoft.EntityFrameworkCore;

namespace Deevenue.Infrastructure.Db;

internal interface IJobResultRepository
{
    Task<bool> DeleteAsync(Guid id);
    Task<int> DeleteOutdatedAsync(DateTime cutOffDateTime);
    Task<IReadOnlySet<JobResultDataSet>> GetAllAsync();
    Task WriteAsync(Guid jobId, string jobTypeName, string errorText);
}

internal record JobResultDataSet(Guid Id, Guid JobId, string JobTypeName, string ErrorText, DateTime InsertedAt);

internal class JobResultRepository(
    DeevenueContext dbContext,
    IDbContextFactory<DeevenueContext> dbContextFactory) : IJobResultRepository
{
    public async Task<bool> DeleteAsync(Guid id)
    {
        var maybeResult = await dbContext.JobResults.SingleOrDefaultAsync(r => r.Id == id);
        if (maybeResult == null)
            return false;

        dbContext.Remove(maybeResult);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteOutdatedAsync(DateTime cutOffDateTime)
    {
        var outdated = await dbContext.JobResults.Where(r => r.InsertedAt < cutOffDateTime).ToListAsync();

        dbContext.RemoveRange(outdated);
        await dbContext.SaveChangesAsync();
        return outdated.Count;
    }

    public async Task<IReadOnlySet<JobResultDataSet>> GetAllAsync()
    {
        var rows = await dbContext.JobResults
            .ToHashSetAsync();

        return rows.Select(r =>
            new JobResultDataSet(r.Id, r.JobId, r.JobTypeName, r.ErrorText, r.InsertedAt)
        ).ToHashSet();
    }

    public async Task WriteAsync(Guid jobId, string jobTypeName, string errorText)
    {
        // We might have got here from an exception, and in that case several things
        // may have gone wrong, resulting in e.g.
        // * there being uninserted, but tracked, entities "stuck" in the current (undisposed) dbContext
        // * the current dbContext being disposed
        //
        // In either case, we need to create our own context to persist this Result.

        using var independentDbContext = await dbContextFactory.CreateDbContextAsync();

        independentDbContext.Add(new JobResult
        {
            JobId = jobId,
            JobTypeName = jobTypeName,
            ErrorText = errorText,
            InsertedAt = DateTime.UtcNow
        });

        await independentDbContext.SaveChangesAsync();
    }
}
