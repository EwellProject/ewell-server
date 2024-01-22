using System.Threading.Tasks;
using EwellServer.EntityEventHandler.Core.Background.BackgroundJobs.BackgroundJobDescriptions;
using EwellServer.EntityEventHandler.Core.Background.Services;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace EwellServer.EntityEventHandler.Core.Background.BackgroundJobs;

public class CancelProjectJob : IAsyncBackgroundJob<CancelProjectJobDescription>, ITransientDependency
{
    private readonly IScriptService _scriptService;
    private readonly ILogger<CancelProjectJob> _logger;

    public CancelProjectJob(IScriptService scriptService, ILogger<CancelProjectJob> logger)
    {
        _scriptService = scriptService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancelProjectJobDescription args)
    {
        _logger.LogInformation("CancelProjectJobDescription args={args}", args);
        await _scriptService.ProcessCancelProjectAsync(args);
    }
}