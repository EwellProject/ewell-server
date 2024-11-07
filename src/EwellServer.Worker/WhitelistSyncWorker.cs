using EwellServer.Common;
using EwellServer.Work;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace EwellServer.Worker;

public class WhitelistSyncWorker : EwellServerWorkBase
{
    public WhitelistSyncWorker(ILogger<ScheduleSyncDataContext> logger,
        AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        IScheduleSyncDataContext scheduleSyncDataContext,
        IOptionsMonitor<WorkerOptions> optionsMonitor) :
        base(logger, timer, serviceScopeFactory, scheduleSyncDataContext, optionsMonitor)
    {
        
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        await _scheduleSyncDataContext.DealAsync(BusinessType);
    }

    protected override WorkerBusinessType BusinessType => WorkerBusinessType.WhitelistSync;
}