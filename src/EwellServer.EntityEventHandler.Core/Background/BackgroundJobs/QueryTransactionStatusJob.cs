using System;
using System.Threading.Tasks;
using EwellServer.EntityEventHandler.Core.Background.BackgroundJobs.BackgroundJobDescriptions;
using EwellServer.EntityEventHandler.Core.Background.Services;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace EwellServer.EntityEventHandler.Core.Background.BackgroundJobs;

public class QueryTransactionStatusJob : IAsyncBackgroundJob<QueryTransactionStatusJobDescription>, ITransientDependency
{
    private readonly ITransactionService _transactionService;
    private readonly IJobEnqueueService _jobEnqueueService;
    private readonly ILogger<QueryTransactionStatusJob> _logger;

    public QueryTransactionStatusJob(ITransactionService transactionService, IJobEnqueueService jobEnqueueService,
        ILogger<QueryTransactionStatusJob> logger)
    {
        _transactionService = transactionService;
        _jobEnqueueService = jobEnqueueService;
        _logger = logger;
    }

    public async Task ExecuteAsync(QueryTransactionStatusJobDescription args)
    {
        var transactionResult = await _transactionService.GetTransactionById(args.ChainName, args.Id);
        if (transactionResult.Status.Equals(EwellConstants.PendingStatus, StringComparison.OrdinalIgnoreCase) ||
            transactionResult.Status.Equals(EwellConstants.PendingValidationStatus, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation($"Transaction: {args.TransactionId} status is pending");
            await _jobEnqueueService.AddJobAsync(args);
            return;
        }

        _logger.LogInformation(
            $"Finished querying transaction:\n{args}\nTransactionResult status:{transactionResult.Status}");
        if (!transactionResult.Status.Equals(EwellConstants.MinedStatus, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning($"Transaction error: {transactionResult.Error}");
        }
    }
}