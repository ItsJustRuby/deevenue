using Deevenue.Infrastructure.Db;
using Quartz;

namespace Deevenue.Infrastructure.Jobs;

internal interface IDeevenueJob : IJob
{
    JobSummaryData GetSummaryData(IJobExecutionContext context);
}

internal abstract class DeevenueJobBase(IJobResultRepository jobRepository) : IDeevenueJob
{
    protected readonly Guid jobId = Guid.NewGuid();
    protected readonly IJobResultRepository jobRepository = jobRepository;

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await ActuallyExecute(context);
        }
        catch (Exception ex)
        {
            await WriteError(ex.Message);
        }
    }

    protected async Task FailAsync(string errorText) => await WriteError(errorText);

    private Task WriteError(string errorText)
        => jobRepository.WriteAsync(jobId, GetType().Name, errorText);

    protected abstract Task ActuallyExecute(IJobExecutionContext context);

    public abstract JobSummaryData GetSummaryData(IJobExecutionContext context);
}
