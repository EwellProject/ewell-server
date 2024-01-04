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
using Microsoft.Extensions.Logging;

namespace EwellServer.Project;

public class UserProjectInfoSyncDataService : ScheduleSyncDataService
{
    private readonly ILogger<ScheduleSyncDataService> _logger;
    private readonly IGraphQLProvider _graphQlProvider;
    private readonly IUserProjectInfoProvider _userProjectInfoProvider;
    private readonly INESTRepository<UserProjectInfoIndex, string> _userProjectInfoIndexRepository;
    private readonly IChainAppService _chainAppService;

    public UserProjectInfoSyncDataService(ILogger<UserProjectInfoSyncDataService> logger,
        IGraphQLProvider graphQlProvider,
        IUserProjectInfoProvider userProjectInfoProvider,
        IChainAppService chainAppService, 
        INESTRepository<UserProjectInfoIndex, string> userProjectInfoIndexRepository)
        : base(logger, graphQlProvider, chainAppService)
    {
        _logger = logger;
        _graphQlProvider = graphQlProvider;
        _userProjectInfoProvider = userProjectInfoProvider;
        _chainAppService = chainAppService;
        _userProjectInfoIndexRepository = userProjectInfoIndexRepository;
    }

    public override async Task<long> SyncIndexerRecordsAsync(string chainId, long lastEndHeight, long newIndexHeight)
    {
        var queryList = await _userProjectInfoProvider.GetSyncUserProjectInfosAsync(chainId, lastEndHeight, 0);
        _logger.LogInformation(
            "SyncUserProjectInfos queryList startBlockHeight: {lastEndHeight} endBlockHeight: {newIndexHeight} count: {count}",
            lastEndHeight, newIndexHeight, queryList?.Count);
        long blockHeight = -1;
        if (queryList.IsNullOrEmpty())
        {
            return 0;
        }

        foreach (var info in queryList)
        {
            blockHeight = Math.Max(blockHeight, info.BlockHeight);
            await _userProjectInfoIndexRepository.AddOrUpdateAsync(info);
        }

        return blockHeight;
    }

    public override async Task<List<string>> GetChainIdsAsync()
    {
        //add multiple chains
        var chainIds = await _chainAppService.GetListAsync();
        return chainIds.ToList();
    }

    public override WorkerBusinessType GetBusinessType()
    {
        return WorkerBusinessType.UserProjectInfoSync;
    }
}