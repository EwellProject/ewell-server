using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using EwellServer.Common;
using EwellServer.Common.AElfSdk;
using EwellServer.Samples.GraphQL;
using EwellServer.Samples.Http;
using Volo.Abp.Caching;
using Volo.Abp.DistributedLocking;

namespace EwellServer.Samples;

public class SampleService : EwellServerAppService, ISampleService
{
    private readonly IDistributedCache<TransactionResultDto> _seedLockCache;
    private readonly ITransactionHttpProvider _transactionHttpProvider;
    private readonly ContractProvider _contractProvider;
    private readonly IActivityProvider _activityProvider;
    private readonly IAbpDistributedLock _distributedLock;


    public SampleService(IDistributedCache<TransactionResultDto> seedLockCache,
        ITransactionHttpProvider transactionHttpProvider, IActivityProvider activityProvider, ContractProvider contractProvider, IAbpDistributedLock distributedLock)
    {
        _seedLockCache = seedLockCache;
        _transactionHttpProvider = transactionHttpProvider;
        _activityProvider = activityProvider;
        _contractProvider = contractProvider;
        _distributedLock = distributedLock;
    }

    // Cache sample
    public async Task<TransactionResultDto> GetTransactionResultWithCacheAsync(string transactionId, string chainId = "AELF")
    {
        return await _seedLockCache.GetOrAddAsync(transactionId,
            () => GetTransactionResultAsync(transactionId, chainId),
            () => new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(10),
            }
        );
    }

    // Distributed Lock sample
    public async Task<TransactionResultDto> GetTransactionResultWithLockAsync(string transactionId, string chainId = "AELF")
    {
        await using var handle =
            await _distributedLock.TryAcquireAsync("LockPrefix" + transactionId);
        AssertHelper.NotNull(handle, "Rate limit exceeded");

        await Task.Delay(2000);
        
        return await GetTransactionResultAsync(transactionId, chainId);
    }

    // Http sample
    public async Task<TransactionResultDto> GetTransactionResultAsync(string transactionId, string chainId = "AELF")
    {
        return await _transactionHttpProvider.GetTransactionResultAsync(transactionId);
    }

    // GraphQL sample
    public Task<IndexerSymbols> GetTokenInfoAsync(string symbol)
    {
        return _activityProvider.GetTokenInfoAsync(symbol);
    }
    
}