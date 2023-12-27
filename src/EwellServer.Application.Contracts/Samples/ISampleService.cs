using System.Threading.Tasks;
using EwellServer.Samples.GraphQL;
using EwellServer.Samples.Http;

namespace EwellServer.Samples;

public interface ISampleService
{
    
    // Cache sample
    Task<TransactionResultDto> GetTransactionResultWithCacheAsync(string transactionId, string chainId = "AELF");
    
    // Distributed lock sample
    Task<TransactionResultDto> GetTransactionResultWithLockAsync(string transactionId, string chainId = "AELF");
    
    // Http sample
    Task<TransactionResultDto> GetTransactionResultAsync(string transactionId, string chainId = "AELF");

    // GraphQL sample
    Task<IndexerSymbols> GetTokenInfoAsync(string symbol);

}