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
            projects = await _userProjectInfoGraphQlProvider.GetProjectListAsync(lastEndHeight, 0, chainId, maxResultCount, skipCount);
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

            await ProcessRegisterProject(projects, chainId);
            await ProcessCancelProject(projects);
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
        return WorkerBusinessType.ProjectInfoSync;
    }

    private async Task ProcessRegisterProject(List<CrowdfundingProjectIndex> projects, string chainId)
    {
        var registerProjects = new List<CrowdfundingProjectIndex>();
        foreach (var project in projects)
        {
            var projectIdExisted = await _graphQlProvider.GetProjectIdAsync(chainId, project.Id);
            if (projectIdExisted == CommonConstant.LongError)
            {
                registerProjects.Add(project);
            }
        }
        
        foreach (var registerProject in registerProjects)
        {
            await _graphQlProvider.SetProjectIdAsync(chainId, registerProject.Id, CommonConstant.LongCommon);
            await _distributedEventBus.PublishAsync(_objectMapper.Map<CrowdfundingProjectIndex, ProjectRegisteredEto>(registerProject));
        }
    }

    private async Task ProcessCancelProject(List<CrowdfundingProjectIndex> projects)
    {
        var cancelProjects = projects.Where(project => project.IsCanceled).ToList();
        foreach (var cancelProject in cancelProjects)
        {
            await _distributedEventBus.PublishAsync(_objectMapper.Map<CrowdfundingProjectIndex, ProjectCanceledEto>(cancelProject));
        }
    }
}