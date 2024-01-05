using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using EwellServer.Chains;
using EwellServer.Common;
using EwellServer.Common.GraphQL;
using EwellServer.Entities;
using EwellServer.Project.Provider;
using Microsoft.Extensions.Logging;

namespace EwellServer.Project;

public class ProjectInfoSyncDataService : ScheduleSyncDataService
{
    private readonly ILogger<ScheduleSyncDataService> _logger;
    private readonly IProjectInfoProvider _projectInfoGraphQLProvider;
    private readonly INESTRepository<CrowdfundingProjectIndex, string> _crowdfundingProjectIndexRepository;
    private readonly IChainAppService _chainAppService;

    public ProjectInfoSyncDataService(ILogger<ProjectInfoSyncDataService> logger,
        IGraphQLProvider graphQlProvider,
        IProjectInfoProvider projectInfoGraphQlProvider,
        IChainAppService chainAppService, 
        INESTRepository<CrowdfundingProjectIndex, string> crowdfundingProjectIndexRepository)
        : base(logger, graphQlProvider)
    {
        _logger = logger;
        _projectInfoGraphQLProvider = projectInfoGraphQlProvider;
        _chainAppService = chainAppService;
        _crowdfundingProjectIndexRepository = crowdfundingProjectIndexRepository;
    }

    public override async Task<long> SyncIndexerRecordsAsync(string chainId, long lastEndHeight, long newIndexHeight)
    {
        var skipCount = 0;
        const int maxResultCount = 800;
        List<CrowdfundingProjectIndex> projects;
        do
        {
            projects = await _projectInfoGraphQLProvider.GetProjectListAsync(lastEndHeight, chainId, maxResultCount, skipCount);
            if (!projects.IsNullOrEmpty())
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