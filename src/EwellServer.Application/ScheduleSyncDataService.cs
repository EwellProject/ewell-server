using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EwellServer.Chains;
using EwellServer.Common;
using EwellServer.Common.GraphQL;
using Microsoft.Extensions.Logging;

namespace EwellServer;

public abstract class ScheduleSyncDataService : IScheduleSyncDataService
{
    private readonly ILogger<ScheduleSyncDataService> _logger;
    private readonly IGraphQLProvider _graphQlProvider;
    private readonly IChainAppService _chainAppService;

    protected ScheduleSyncDataService(ILogger<ScheduleSyncDataService> logger, IGraphQLProvider graphQlProvider,
        IChainAppService chainAppService)
    {
        _logger = logger;
        _graphQlProvider = graphQlProvider;
        _chainAppService = chainAppService;
    }


    public async Task DealDataAsync()
    {
        var businessType = GetBusinessType();
        var chainIds = await GetChainIdsAsync();
        //handle multiple chains
        foreach (var chainId in chainIds)
        {
            try
            {
                var lastEndHeight = await _graphQlProvider.GetLastEndHeightAsync(chainId, businessType);
                var newIndexHeight = await _graphQlProvider.GetIndexBlockHeightAsync(chainId);
                _logger.LogInformation(
                    "Start deal data for businessType: {businessType} chainId: {chainId} lastEndHeight: {lastEndHeight} newIndexHeight: {newIndexHeight}",
                    businessType, chainId, lastEndHeight, newIndexHeight);
                var blockHeight = await SyncIndexerRecordsAsync(chainId, lastEndHeight, newIndexHeight);

                if (blockHeight > 0)
                {
                    await _graphQlProvider.SetLastEndHeightAsync(chainId, businessType, blockHeight);
                    _logger.LogInformation(
                        "End deal data for businessType: {businessType} chainId: {chainId} lastEndHeight: {BlockHeight}",
                        businessType, chainId, blockHeight);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DealDataAsync error businessType:{businessType} chainId: {chainId}",
                    businessType.ToString(), chainId);
            }
        }
    }

    public abstract Task<long> SyncIndexerRecordsAsync(string chainId,long lastEndHeight, long newIndexHeight);
    /**
     * different businesses obtain different multiple chains
     */
    public abstract Task<List<string>> GetChainIdsAsync();

    public abstract WorkerBusinessType GetBusinessType();
}