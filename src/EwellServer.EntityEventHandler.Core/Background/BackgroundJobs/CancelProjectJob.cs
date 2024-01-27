using System;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using EwellServer.Entities;
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
    private readonly INESTRepository<CrowdfundingProjectIndex, string> _crowdfundingProjectIndexRepository;

    public CancelProjectJob(IScriptService scriptService, ILogger<CancelProjectJob> logger, INESTRepository<CrowdfundingProjectIndex, string> crowdfundingProjectIndexRepository)
    {
        _scriptService = scriptService;
        _logger = logger;
        _crowdfundingProjectIndexRepository = crowdfundingProjectIndexRepository;
    }

    public async Task ExecuteAsync(CancelProjectJobDescription args)
    {
        _logger.LogInformation("ExecuteAsyncCancelProjectJobDescription args={args}", args);
        try
        {
            _logger.LogInformation("ExecuteAsyncCancelProjectJobDescriptionAddEs args={args}", args);
            await _crowdfundingProjectIndexRepository.AddOrUpdateAsync(new CrowdfundingProjectIndex
            {
                Id = args.Id,
                PeriodDuration = 3
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e,"ExecuteAsyncCancelProjectJobDescription id={id}",args.Id);
        }
        await _scriptService.ProcessCancelProjectAsync(args);
    }
}