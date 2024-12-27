using Deevenue.Infrastructure.Db;

namespace Deevenue.Infrastructure.Jobs;

public interface IJobResultService
{
    Task<bool> DeleteAsync(Guid id);
    Task<IReadOnlySet<JobResultViewModel>> GetAllAsync();
}
public record JobResultViewModel(Guid Id, Guid JobId, string JobKindName, string ErrorText, DateTime InsertedAt);

internal class JobResultService(IJobResultRepository repository) : IJobResultService
{
    public async Task<IReadOnlySet<JobResultViewModel>> GetAllAsync()
    {
        var results = await repository.GetAllAsync();
        return results.Select(r =>
            new JobResultViewModel(
                r.Id, r.JobId, JobNames.JobKindNameByJobTypeName[r.JobTypeName], r.ErrorText, r.InsertedAt)
        ).ToHashSet();
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return repository.DeleteAsync(id);
    }
}
