using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using EwellServer.Chains;
using EwellServer.Common;
using EwellServer.Common.GraphQL;
using EwellServer.Entities;
using EwellServer.Project.Provider;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver.Linq;
using Nest;

namespace EwellServer.Project;

public class WhitelistSyncDataService : ScheduleSyncDataService
{
    private readonly ILogger<ScheduleSyncDataService> _logger;
    private readonly IUserProjectInfoProvider _userProjectInfoGraphQlProvider;
    private readonly INESTRepository<CrowdfundingProjectIndex, string> _crowdfundingProjectIndexRepository;
    private readonly IChainAppService _chainAppService;

    public WhitelistSyncDataService(ILogger<UserRecordSyncDataService> logger,
        IGraphQLProvider graphQlProvider,
        IUserProjectInfoProvider userProjectInfoGraphQlProvider,
        IChainAppService chainAppService, 
        INESTRepository<CrowdfundingProjectIndex, string> crowdfundingProjectRepository)
        : base(logger, graphQlProvider)
    {
        _logger = logger;
        _userProjectInfoGraphQlProvider = userProjectInfoGraphQlProvider;
        _chainAppService = chainAppService;
        _crowdfundingProjectIndexRepository = crowdfundingProjectRepository;
    }

    public override async Task<long> SyncIndexerRecordsAsync(string chainId, long lastEndHeight, long newIndexHeight)
    {
        var skipCount = 0;
        const int maxResultCount = 800;
        List<Whitelist> whitelists;
        do
        {
            whitelists = await _userProjectInfoGraphQlProvider.GetWhitelistListAsync(lastEndHeight, chainId, maxResultCount, skipCount);
            _logger.LogInformation("SyncWhitelistInfos GetWhitelistListAsync startBlockHeight: {lastEndHeight} skipCount: {skipCount} count: {count}", 
                lastEndHeight, skipCount, whitelists.Count);
            if (whitelists.IsNullOrEmpty())
            {
                break;
            }
            
            var maxCurrentBlockHeight = whitelists.Select(x => x.BlockHeight).Max();
            if (maxCurrentBlockHeight == lastEndHeight)
            {
                skipCount += whitelists.Select(x => x.BlockHeight == lastEndHeight).Count();
            }
            else
            {
                skipCount = whitelists.Select(x => x.BlockHeight == maxCurrentBlockHeight).Count();
                lastEndHeight = maxCurrentBlockHeight;
            }

            var whitelistIds = whitelists.Select(x => x.Id).ToList();
            var mustQuery = new List<Func<QueryContainerDescriptor<CrowdfundingProjectIndex>, QueryContainer>>
            {
                q => q.Terms(i => i.Field(f => f.WhitelistId).Terms(whitelistIds)),
                q => q.Term(i => i.Field(f => f.ChainId).Value(chainId))
            };
            var projects = (await _crowdfundingProjectIndexRepository.GetListAsync(
                    f => f.Bool(b => b.Must(mustQuery))))
                .Item2.ToList();
            if (!projects.IsNullOrEmpty())
            {
                foreach (var crowdfundingProjectIndex in projects)
                {
                    var latest = whitelists.First(whitelist => crowdfundingProjectIndex.WhitelistId == whitelist.Id);
                    crowdfundingProjectIndex.IsEnableWhitelist = latest.IsAvailable;
                }    
                await _crowdfundingProjectIndexRepository.BulkAddOrUpdateAsync(projects);
            }
        } while (!whitelists.IsNullOrEmpty());

        return lastEndHeight;
    }

    public override async Task<List<string>> GetChainIdsAsync()
    {
        var chainIds = await _chainAppService.GetListAsync();
        return chainIds.ToList();
    }

    public override WorkerBusinessType GetBusinessType()
    {
        return WorkerBusinessType.WhitelistSync;
    }
}