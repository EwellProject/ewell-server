using EwellServer.Common;
using EwellServer.Work;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace EwellServer.Worker.Worker;

public abstract class EwellServerWorkBase : AsyncPeriodicBackgroundWorkerBase
{
    protected abstract WorkerBusinessType BusinessType { get; }
    
    protected readonly ILogger<ScheduleSyncDataContext> _logger;
    protected readonly IScheduleSyncDataContext _scheduleSyncDataContext;
    private const int DefaultPeriod = 30000;

    protected EwellServerWorkBase(ILogger<ScheduleSyncDataContext> logger,
        AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        IScheduleSyncDataContext scheduleSyncDataContext,
        IOptionsMonitor<WorkerOptions> optionsMonitor) :
        base(timer, serviceScopeFactory)
    {
        _logger = logger;
        _scheduleSyncDataContext = scheduleSyncDataContext;
        timer.Period = optionsMonitor.CurrentValue.GetWorkerSettings(BusinessType) != null ?
            optionsMonitor.CurrentValue.GetWorkerSettings(BusinessType).TimePeriod : DefaultPeriod;
        
        //to change timer Period if the WorkerOptions has changed.
        optionsMonitor.OnChange((newOptions, _) =>
        {
            var workerSetting = newOptions.GetWorkerSettings(BusinessType);
            timer.Period = workerSetting.TimePeriod;
            if (workerSetting.OpenSwitch)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
            }

            _logger.LogInformation(
                "The workerSetting of Worker {BusinessType} has changed to Period = {Period} ms, OpenSwitch = {OpenSwitch}.",
                BusinessType, timer.Period, workerSetting.OpenSwitch);
        });
    }
    
}