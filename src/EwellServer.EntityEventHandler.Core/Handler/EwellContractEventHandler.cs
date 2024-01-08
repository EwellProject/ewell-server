using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using EwellServer.Common;
using EwellServer.Entities;
using EwellServer.EntityEventHandler.Core.Background.BackgroundJobs.BackgroundJobDescriptions;
using EwellServer.EntityEventHandler.Core.Background.Services;
using EwellServer.Etos;
using Microsoft.Extensions.Logging;
using Nest;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace EwellServer.EntityEventHandler.Core.Handler;

public class EwellContractEventHandler : IDistributedEventHandler<ProjectRegisteredEto>, IDistributedEventHandler<ProjectCanceledEto>, ITransientDependency
{
    private readonly IJobEnqueueService _jobEnqueueService;
    private readonly INESTRepository<UserProjectInfoIndex, string> _userProjectInfoIndexRepository;
    private readonly ILogger<EwellContractEventHandler> _logger;
    
    public EwellContractEventHandler(IJobEnqueueService jobEnqueueService, 
        INESTRepository<UserProjectInfoIndex, string> userProjectInfoIndexRepository, 
        ILogger<EwellContractEventHandler> logger)
    {
        _jobEnqueueService = jobEnqueueService;
        _userProjectInfoIndexRepository = userProjectInfoIndexRepository;
        _logger = logger;
    }

    public async Task HandleEventAsync(ProjectRegisteredEto eto)
    {
        await AddUnlockJobAsync(eto.ChainId, eto.ProjectId, eto.TotalPeriod,
            eto.PeriodDuration,
            new Timestamp
            {
                Seconds = TimeHelper.GetTimeStampFromDateTime(eto.EndTime)
            });
    }
    
    public async Task HandleEventAsync(ProjectCanceledEto eto)
    {
        await AddProjectCancelJobAsync(eto.CrowdfundingProjectId, eto.ChainId);
    }
    
    private async Task AddUnlockJobAsync(string chainName, string projectId, int totalPeriod, long periodDuration,
        Timestamp endTime)
    {
        _logger.LogInformation("AddUnlockJobAsync Id={chainName} ChainName={projectId}, TotalPeriod={totalPeriod}, PeriodDuration={periodDuration}, EndTime={endTime}",
            chainName, projectId, totalPeriod, periodDuration, endTime);
        var jobStartTime = endTime.ToDateTimeOffset();
        await _jobEnqueueService.AddJobAtFirstTimeAsync(chainName, projectId, jobStartTime, 0, totalPeriod,
            periodDuration);
    }
    
    private async Task AddProjectCancelJobAsync(string id, string chainName)
    {
        _logger.LogInformation("AddProjectCancelJobAsync Id={id} ChainName={chainName}", id, chainName);
        var mustQuery = new List<Func<QueryContainerDescriptor<UserProjectInfoIndex>, QueryContainer>>
        {
            q => q.Term(i => i.Field(f => f.CrowdfundingProjectId).Value(id)),
            q => q.Range(i => i.Field(f => f.InvestAmount).GreaterThan(0))
        };
        
        var cancelProjectJobDescription = new CancelProjectJobDescription
        {
            ChainName = chainName,
            Id = id,
            Users = (await _userProjectInfoIndexRepository
                .GetListAsync(f => f.Bool(b => b.Must(mustQuery)))).Item2
                .Select(u => u.User).ToList()
        };
        await _jobEnqueueService.AddJobAsync(cancelProjectJobDescription);
    }
}