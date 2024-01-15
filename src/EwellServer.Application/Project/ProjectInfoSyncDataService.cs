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
using EwellServer.Token;
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
    private readonly ITokenService _tokenService;
    private readonly IProjectGrainService _projectGrainService;

    public ProjectInfoSyncDataService(ILogger<ProjectInfoSyncDataService> logger,
        IGraphQLProvider graphQlProvider,
        IUserProjectInfoProvider userProjectInfoGraphQlProvider,
        IChainAppService chainAppService, 
        INESTRepository<CrowdfundingProjectIndex, string> crowdfundingProjectIndexRepository,
        IDistributedEventBus distributedEventBus, IObjectMapper objectMapper, ITokenService tokenService,
        IProjectGrainService projectGrainService)
        : base(logger, graphQlProvider)
    {
        _logger = logger;
        _graphQlProvider = graphQlProvider;
        _userProjectInfoGraphQlProvider = userProjectInfoGraphQlProvider;
        _chainAppService = chainAppService;
        _crowdfundingProjectIndexRepository = crowdfundingProjectIndexRepository;
        _distributedEventBus = distributedEventBus;
        _objectMapper = objectMapper;
        _tokenService = tokenService;
        _projectGrainService = projectGrainService;
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
            //set token info
            await ProcessTokenInfo(projects);
            await ProcessRegisterProject(projects, chainId);
            await ProcessCancelProject(projects);
            await _crowdfundingProjectIndexRepository.BulkAddOrUpdateAsync(projects);
        } while (!projects.IsNullOrEmpty());

        return lastEndHeight;
    }

    private async Task ProcessTokenInfo(List<CrowdfundingProjectIndex> projects)
    {
        foreach (var p in projects)
        {
            var toRaiseToken = await _tokenService
                .GetTokenAsync(p.ToRaiseToken.ChainId, p.ToRaiseToken.Symbol);
            var crowdFundingIssueToken = await _tokenService
                .GetTokenAsync(p.CrowdFundingIssueToken.ChainId, p.CrowdFundingIssueToken.Symbol);
            p.ToRaiseToken = _objectMapper.Map<TokenGrainDto, TokenBasicInfo>(toRaiseToken);
            p.CrowdFundingIssueToken = _objectMapper.Map<TokenGrainDto, TokenBasicInfo>(crowdFundingIssueToken);
        }
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
            var projectIdExisted = await _projectGrainService.GetProjectExistAsync(chainId, project.Id);
            if (projectIdExisted)
            {
                _logger.LogInformation("ProcessUpdateProject chainId: {chainId} projectId: {projectId}", chainId, project.Id);
                continue;
            }

            _logger.LogInformation("ProcessRegisterProject chainId: {chainId} projectId: {projectId}", chainId, project.Id);
            registerProjects.Add(project);
        }
        
        foreach (var registerProject in registerProjects)
        {
            await _projectGrainService.SetProjectExistAsync(chainId, registerProject.Id, true);
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