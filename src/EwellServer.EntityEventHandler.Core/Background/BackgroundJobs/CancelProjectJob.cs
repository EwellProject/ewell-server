using System.Threading.Tasks;
using EwellServer.EntityEventHandler.Core.Background.BackgroundJobs.BackgroundJobDescriptions;
using EwellServer.EntityEventHandler.Core.Background.Services;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace EwellServer.EntityEventHandler.Core.Background.BackgroundJobs;

public class CancelProjectJob : IAsyncBackgroundJob<CancelProjectJobDescription>, ITransientDependency
{
    private readonly IScriptService _scriptService;

    public CancelProjectJob(IScriptService scriptService)
    {
        _scriptService = scriptService;
    }

    public async Task ExecuteAsync(CancelProjectJobDescription args)
    {
        await _scriptService.ProcessCancelProjectAsync(args);
    }
}