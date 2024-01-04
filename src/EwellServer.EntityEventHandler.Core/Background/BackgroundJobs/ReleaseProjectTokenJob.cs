using System.Threading.Tasks;
using EwellServer.EntityEventHandler.Core.Background.BackgroundJobs.BackgroundJobDescriptions;
using EwellServer.EntityEventHandler.Core.Background.Services;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace EwellServer.EntityEventHandler.Core.Background.BackgroundJobs;

public class ReleaseProjectTokenJob : IAsyncBackgroundJob<ReleaseProjectTokenJobDescription>, ITransientDependency
{
    private readonly IScriptService _scriptService;
    private readonly IJobEnqueueService _jobEnqueueService;

    public ReleaseProjectTokenJob(IScriptService scriptService,
        IJobEnqueueService jobEnqueueService)
    {
        _scriptService = scriptService;
        _jobEnqueueService = jobEnqueueService;
    }

    public async Task ExecuteAsync(ReleaseProjectTokenJobDescription args)
    {
        var nextPeriod = await _scriptService.ProcessReleaseTokenAsync(args);
        if (nextPeriod == args.TotalPeriod)
            return;

        args.IsNeedUnlockLiquidity = false;
        args.CurrentPeriod = nextPeriod;
        await _jobEnqueueService.AddJobAsync(args);
    }
}