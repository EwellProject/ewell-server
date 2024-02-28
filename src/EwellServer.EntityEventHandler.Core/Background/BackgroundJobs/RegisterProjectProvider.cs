using System;
using System.Threading.Tasks;
using EwellServer.EntityEventHandler.Core.Background.BackgroundJobs.BackgroundJobDescriptions;
using EwellServer.EntityEventHandler.Core.Background.Services;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Hangfire;

namespace EwellServer.EntityEventHandler.Core.Background.BackgroundJobs;

public interface IRegisterProjectProvider : ISingletonDependency
{
    Task ExecuteAsync(ReleaseProjectTokenJobDescription args);
}

public class RegisterProjectProvider : IRegisterProjectProvider
{
    private readonly IScriptService _scriptService;
    private readonly ILogger<RegisterProjectProvider> _logger;
    private readonly IJobEnqueueService _jobEnqueueService;

    public RegisterProjectProvider(IScriptService scriptService, ILogger<RegisterProjectProvider> logger, IJobEnqueueService jobEnqueueService)
    {
        _scriptService = scriptService;
        _logger = logger;
        _jobEnqueueService = jobEnqueueService;
    }
    
    public async Task ExecuteAsync(ReleaseProjectTokenJobDescription args)
    {
        _logger.LogInformation("ExecuteAsyncReleaseProjectTokenJobDescription args={args}", args);
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