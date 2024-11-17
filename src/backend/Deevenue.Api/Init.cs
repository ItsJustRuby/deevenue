using Deevenue.Domain;
using Deevenue.Infrastructure;
using Deevenue.Infrastructure.Jobs;
using Microsoft.AspNetCore.Http.Features;
using Quartz;

namespace Deevenue.Api;

internal static class ApiSetup
{
    internal static void AddDeevenueApi(this IServiceCollection services)
    {
        var adapter = new ApiDependencyInjection(services);

        adapter.Services.AddTransient<ISfwService, SfwService>();
        adapter.Services.AddSingleton<IJobScheduler, ApiJobScheduler>();

        adapter.Services.Configure<FormOptions>(o =>
        {
            o.ValueLengthLimit = int.MaxValue;
            o.MultipartBodyLengthLimit = long.MaxValue;
            o.MultipartBoundaryLengthLimit = int.MaxValue;
            o.MultipartHeadersCountLimit = int.MaxValue;
            o.MultipartHeadersLengthLimit = int.MaxValue;
        });

        adapter.AddInfrastructure();
    }

    private class ApiDependencyInjection(IServiceCollection services) : IDependencyInjection
    {
        public IServiceCollection Services => services;

        public IConfiguration Config => Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
    }

    private class ApiJobScheduler(ISchedulerFactory quartzSchedulerFactory) : IJobScheduler
    {
        public async Task<IScheduler> UnwrapAsync() => await quartzSchedulerFactory.GetScheduler();
    }
}
