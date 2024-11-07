using System.Threading.Tasks;
using EwellServer.EntityEventHandler.Core.Background.BackgroundJobs.BackgroundJobDescriptions;
using EwellServer.EntityEventHandler.Core.Background.Services;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace EwellServer.EntityEventHandler.Core.Background.BackgroundJobs;

public class ReleaseProjectTokenJob : IAsyncBackgroundJob<ReleaseProjectTokenJobDescription>, ITransientDependency
{
    private readonly IScriptService _scriptService;
    private readonly IJobEnqueueService _jobEnqueueService;
    private readonly ILogger<ReleaseProjectTokenJob> _logger;

    public ReleaseProjectTokenJob(IScriptService scriptService,
        IJobEnqueueService jobEnqueueService, ILogger<ReleaseProjectTokenJob> logger)
    {
        _scriptService = scriptService;
        _jobEnqueueService = jobEnqueueService;
        _logger = logger;
    }

    public async Task ExecuteAsync(ReleaseProjectTokenJobDescription args)
    {
        _logger.LogInformation("ReleaseProjectTokenJobDescription args={args}", args);
        var nextPeriod = await _scriptService.ProcessReleaseTokenAsync(args);
        if (nextPeriod == args.TotalPeriod)
        {
            return;
        }

        args.IsNeedUnlockLiquidity = false;
        args.CurrentPeriod = nextPeriod;
        await _jobEnqueueService.AddJobAsync(args);
    }
}