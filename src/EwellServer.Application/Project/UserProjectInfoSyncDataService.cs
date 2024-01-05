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
    private readonly IUserProjectInfoProvider _userProjectInfoProvider;
    private readonly INESTRepository<UserProjectInfoIndex, string> _userProjectInfoIndexRepository;
    private readonly IChainAppService _chainAppService;

    public UserProjectInfoSyncDataService(ILogger<UserProjectInfoSyncDataService> logger,
        IGraphQLProvider graphQlProvider,
        IUserProjectInfoProvider userProjectInfoProvider,
        IChainAppService chainAppService,
        INESTRepository<UserProjectInfoIndex, string> userProjectInfoIndexRepository)
        : base(logger, graphQlProvider)
    {
        _logger = logger;
        _userProjectInfoProvider = userProjectInfoProvider;
        _chainAppService = chainAppService;
        _userProjectInfoIndexRepository = userProjectInfoIndexRepository;
    }

    public override async Task<long> SyncIndexerRecordsAsync(string chainId, long lastEndHeight, long newIndexHeight)
    {
        var skipCount = 0;
        long blockHeight = -1;
        List<UserProjectInfoIndex> queryList;
        do
        {
            queryList = await _userProjectInfoProvider.GetSyncUserProjectInfosAsync(skipCount, chainId, lastEndHeight,
                0);
            _logger.LogInformation(
                "SyncUserProjectInfos queryList skipCount {skipCount} startBlockHeight: {lastEndHeight} endBlockHeight: {newIndexHeight} count: {count}",
                skipCount, lastEndHeight, newIndexHeight, queryList?.Count);
            if (queryList.IsNullOrEmpty())
            {
                break;
            }

            foreach (var info in queryList)
            {
                blockHeight = Math.Max(blockHeight, info.BlockHeight);
                await _userProjectInfoIndexRepository.AddOrUpdateAsync(info);
            }

            skipCount += queryList.Count;
        } while (!queryList.IsNullOrEmpty());

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