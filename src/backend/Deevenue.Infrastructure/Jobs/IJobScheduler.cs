using Quartz;

namespace Deevenue.Infrastructure.Jobs;

public interface IJobScheduler
{
    Task<IScheduler> UnwrapAsync();
}
