using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using EwellServer.Chains;
using EwellServer.Common;
using EwellServer.Common.GraphQL;
using EwellServer.Entities;
using EwellServer.Etos;
using EwellServer.Project.Provider;
using Microsoft.Extensions.Logging;
using Nest;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace EwellServer.Project;

public class ProjectInfoSyncDataService : ScheduleSyncDataService
{
    private readonly ILogger<ScheduleSyncDataService> _logger;
    private readonly IUserProjectInfoProvider _userProjectInfoGraphQlProvider;
    private readonly INESTRepository<CrowdfundingProjectIndex, string> _crowdfundingProjectIndexRepository;
    private readonly IChainAppService _chainAppService;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly IObjectMapper _objectMapper;
    private readonly IGraphQLProvider _graphQlProvider;

    public ProjectInfoSyncDataService(ILogger<ProjectInfoSyncDataService> logger,
        IGraphQLProvider graphQlProvider,
        IUserProjectInfoProvider userProjectInfoGraphQlProvider,
        IChainAppService chainAppService, 
        INESTRepository<CrowdfundingProjectIndex, string> crowdfundingProjectIndexRepository,
        IDistributedEventBus distributedEventBus, IObjectMapper objectMapper)
        : base(logger, graphQlProvider)
    {
        _logger = logger;
        _graphQlProvider = graphQlProvider;
        _userProjectInfoGraphQlProvider = userProjectInfoGraphQlProvider;
        _chainAppService = chainAppService;
        _crowdfundingProjectIndexRepository = crowdfundingProjectIndexRepository;
        _distributedEventBus = distributedEventBus;
        _objectMapper = objectMapper;
    }

    public override async Task<long> SyncIndexerRecordsAsync(string chainId, long lastEndHeight, long newIndexHeight)
    {
        var skipCount = 0;
        const int maxResultCount = 800;
        List<CrowdfundingProjectIndex> projects;
        do
        {
            projects = await _userProjectInfoGraphQlProvider.GetProjectListAsync(lastEndHeight, chainId, maxResultCount, skipCount);
            _logger.LogInformation("SyncProjectInfos GetProjectListAsync startBlockHeight: {lastEndHeight} skipCount: {skipCount} count: {count}", 
                lastEndHeight, skipCount, projects.Count);
            if (projects.IsNullOrEmpty())
            {
                break;
            }
            
            var maxCurrentBlockHeight = projects.Select(x => x.BlockHeight).Max();
            if (maxCurrentBlockHeight == lastEndHeight)
            {
                skipCount += projects.Select(x => x.BlockHeight == lastEndHeight).Count();
            }
            else
            {
                skipCount = projects.Select(x => x.BlockHeight == maxCurrentBlockHeight).Count();
                lastEndHeight = maxCurrentBlockHeight;
            }


            var projectIds = projects.Select(x => x.Id).ToList();
            var registerProjectIds = new List<string>();
            foreach (var projectId in projectIds)
            {
                var projectIdExisted = await _graphQlProvider.GetProjectIdAsync(chainId, projectId);
                if (projectIdExisted == CommonConstant.LongError)
                {
                    registerProjectIds.Add(projectId);
                }
            }
            var registerProjects = projects.Where(project => registerProjectIds.Contains(project.Id) && !project.IsCanceled).ToList();
            var cancelProjects = projects.Where(project => project.IsCanceled).ToList();
            foreach (var registerProject in registerProjects)
            {
                await _graphQlProvider.SetProjectIdAsync(chainId, registerProject.Id, CommonConstant.LongCommon);
                await _distributedEventBus.PublishAsync(_objectMapper.Map<CrowdfundingProjectIndex, ProjectRegisteredEto>(registerProject));
            }
            foreach (var cancelProject in cancelProjects)
            {
                await _distributedEventBus.PublishAsync(_objectMapper.Map<CrowdfundingProjectIndex, ProjectCanceledEto>(cancelProject));
            }
            
            await _crowdfundingProjectIndexRepository.BulkAddOrUpdateAsync(projects);
        } while (!projects.IsNullOrEmpty());

        return lastEndHeight;
    }

    public override async Task<List<string>> GetChainIdsAsync()
    {
        var chainIds = await _chainAppService.GetListAsync();
        return chainIds.ToList();
    }

    public override WorkerBusinessType GetBusinessType()
    {
        return WorkerBusinessType.UserProjectInfoSync;
    }
}