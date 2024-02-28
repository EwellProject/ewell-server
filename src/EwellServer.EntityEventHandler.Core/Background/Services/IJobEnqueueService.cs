using System;
using System.Threading.Tasks;
using EwellServer.EntityEventHandler.Core.Background.BackgroundJobs.BackgroundJobDescriptions;

namespace EwellServer.EntityEventHandler.Core.Background.Services;

public interface IJobEnqueueService
{
    Task AddJobAtFirstTimeAsync(string chainName, string projectId, DateTimeOffset startTime, int currentPeriod, int totalPeriod,
        long periodDuration);

    Task AddJobAsync(ReleaseProjectTokenJobDescription releaseProjectTokenJobDescription);

    Task AddJobAsync(QueryTransactionStatusJobDescription transactionStatusJobDescription);
    Task AddJobAsync(CancelProjectJobDescription cancelProjectJobDescription);
}