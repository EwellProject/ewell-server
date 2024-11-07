using System.Threading.Tasks;
using EwellServer.EntityEventHandler.Core.Background.BackgroundJobs.BackgroundJobDescriptions;
using EwellServer.EntityEventHandler.Core.Background.Services;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace EwellServer.EntityEventHandler.Core.Background.BackgroundJobs;

public interface ICancelProjectProvider : ISingletonDependency
{
    Task ExecuteAsync(CancelProjectJobDescription args);
}

public class CancelProjectProvider : ICancelProjectProvider
{
    private readonly IScriptService _scriptService;
    private readonly ILogger<CancelProjectProvider> _logger;

    public CancelProjectProvider(IScriptService scriptService, ILogger<CancelProjectProvider> logger)
    {
        _scriptService = scriptService;
        _logger = logger;
    }
    
    public async Task ExecuteAsync(CancelProjectJobDescription args)
    {
        _logger.LogInformation("ExecuteAsyncCancelProjectJobDescription args={args}", args);
        await _scriptService.ProcessCancelProjectAsync(args);
    }
}