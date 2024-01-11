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

public class UserRecordSyncDataService : ScheduleSyncDataService
{
    private readonly ILogger<ScheduleSyncDataService> _logger;
    private readonly IUserProjectInfoProvider _userProjectInfoGraphQlProvider;
    private readonly INESTRepository<UserRecordIndex, string> _userRecordRepository;
    private readonly IChainAppService _chainAppService;

    public UserRecordSyncDataService(ILogger<UserRecordSyncDataService> logger,
        IGraphQLProvider graphQlProvider,
        IUserProjectInfoProvider userProjectInfoGraphQlProvider,
        IChainAppService chainAppService, 
        INESTRepository<UserRecordIndex, string> userRecordRepository)
        : base(logger, graphQlProvider)
    {
        _logger = logger;
        _userProjectInfoGraphQlProvider = userProjectInfoGraphQlProvider;
        _chainAppService = chainAppService;
        _userRecordRepository = userRecordRepository;
    }

    public override async Task<long> SyncIndexerRecordsAsync(string chainId, long lastEndHeight, long newIndexHeight)
    {
        var skipCount = 0;
        const int maxResultCount = 800;
        List<UserRecordIndex> userRecords;
        do
        {
            userRecords = await _userProjectInfoGraphQlProvider.GetUserRecordListAsync(lastEndHeight, 0, chainId, maxResultCount, skipCount);
            _logger.LogInformation("SyncUserRecordInfos GetUserRecordListAsync startBlockHeight: {lastEndHeight} skipCount: {skipCount} count: {count}", 
                lastEndHeight, skipCount, userRecords.Count);
            if (userRecords.IsNullOrEmpty())
            {
                break;
            }
            
            var maxCurrentBlockHeight = userRecords.Select(x => x.BlockHeight).Max();
            if (maxCurrentBlockHeight == lastEndHeight)
            {
                skipCount += userRecords.Select(x => x.BlockHeight == lastEndHeight).Count();
            }
            else
            {
                skipCount = userRecords.Select(x => x.BlockHeight == maxCurrentBlockHeight).Count();
                lastEndHeight = maxCurrentBlockHeight;
            }

            await _userRecordRepository.BulkAddOrUpdateAsync(userRecords);
        } while (!userRecords.IsNullOrEmpty());

        return lastEndHeight;
    }

    public override async Task<List<string>> GetChainIdsAsync()
    {
        var chainIds = await _chainAppService.GetListAsync();
        return chainIds.ToList();
    }

    public override WorkerBusinessType GetBusinessType()
    {
        return WorkerBusinessType.UserRecordSync;
    }
}