using System;
using System.Threading.Tasks;
using EwellServer.EntityEventHandler.Core.Background.BackgroundJobs.BackgroundJobDescriptions;
using EwellServer.EntityEventHandler.Core.Background.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace EwellServer.EntityEventHandler.Core.Background.Services.Impl;

public class JobEnqueueService : IJobEnqueueService, ITransientDependency
{
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly ILogger<JobEnqueueService> _logger;
    private readonly int _checkInterval;

    public JobEnqueueService(IBackgroundJobManager backgroundJobManager,
        ILogger<JobEnqueueService> logger, IOptionsSnapshot<EwellOption> optionsSnapshot)
    {
        _backgroundJobManager = backgroundJobManager;
        _logger = logger;
        _checkInterval = optionsSnapshot.Value.CheckTransactionInterval;
    }

    public async Task AddJobAtFirstTimeAsync(string chainName, string projectId, DateTimeOffset startTime,
        int currentPeriod, int totalPeriod, long periodDuration)
    {
        var delay = startTime.ToUnixTimeSeconds() - DateTimeOffset.Now.ToUnixTimeSeconds();
        if (delay < 0)
        {
            LogWarning(chainName, projectId, currentPeriod, totalPeriod, delay);
            delay = 1;
        }
        
        var jobInfo = new ReleaseProjectTokenJobDescription
        {
            ChainName = chainName,
            Id = projectId,
            CurrentPeriod = currentPeriod,
            TotalPeriod = totalPeriod,
            PeriodDuration = periodDuration,
            StartTime = startTime,
            IsNeedUnlockLiquidity = true
        };
        var executionTime = DateTimeOffset.UtcNow.AddSeconds(delay);
        _logger.LogInformation(
            "AddJobAtFirstTimeAsyncBegin Job:{jobInfo} Expect execution time: {executionTime}", jobInfo, executionTime);
        try
        {
            var jobId = await _backgroundJobManager.EnqueueAsync(jobInfo, BackgroundJobPriority.Normal, TimeSpan.FromSeconds(delay));
            _logger.LogInformation(
                "AddJobAtFirstTimeAsyncEnd Job:{jobInfo} Expect execution time: {executionTime} JobId:{jobId}", jobInfo, executionTime, jobId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "AddJobAtFirstTimeAsyncException Job:{jobInfo} Expect execution time: {executionTime}", jobInfo, executionTime);
        }
    }

    public async Task AddJobAsync(ReleaseProjectTokenJobDescription releaseProjectTokenJobDescription)
    {
        var triggerTimestamp = releaseProjectTokenJobDescription.StartTime.ToUnixTimeSeconds() +
                               releaseProjectTokenJobDescription.CurrentPeriod * releaseProjectTokenJobDescription.PeriodDuration;
        var delay = triggerTimestamp - DateTimeOffset.Now.ToUnixTimeSeconds();
        var executionTime = DateTimeOffset.UtcNow.AddSeconds(delay);
        _logger.LogInformation("AddJobAsyncReleaseProjectTokenJobDescription Job:{releaseProjectTokenJobDescription} Expect execution time: {executionTime}, delay={delay}", 
            releaseProjectTokenJobDescription, executionTime, delay);
        if (delay < 0)
        {
            LogWarning(releaseProjectTokenJobDescription.ChainName, releaseProjectTokenJobDescription.Id, releaseProjectTokenJobDescription.CurrentPeriod, releaseProjectTokenJobDescription.TotalPeriod, delay);
            return;
        }

        LogNewJob(releaseProjectTokenJobDescription, delay);
        await _backgroundJobManager.EnqueueAsync(releaseProjectTokenJobDescription, BackgroundJobPriority.Normal,
            TimeSpan.FromSeconds(delay));
    }

    public async Task AddJobAsync(QueryTransactionStatusJobDescription transactionStatusJobDescription)
    {
        var delay = _checkInterval;
        var executionTime = DateTimeOffset.UtcNow.AddSeconds(delay);
        _logger.LogInformation(
            $"Add New QueryTransactionStatusJob Job:\n{transactionStatusJobDescription}\nExpect execution time: {executionTime}");
        await _backgroundJobManager.EnqueueAsync(transactionStatusJobDescription, BackgroundJobPriority.Normal,
            TimeSpan.FromSeconds(delay));
    }

    public async Task AddJobAsync(CancelProjectJobDescription cancelProjectJobDescription)
    {
        const int delay = 1;
        var executionTime = DateTimeOffset.UtcNow.AddSeconds(delay);
        _logger.LogInformation(
            "CancelProjectJobDescriptionBegin Job:{cancelProjectJobDescription} Expect execution time: {executionTime}", 
            cancelProjectJobDescription, executionTime);
        var jobId = await _backgroundJobManager.EnqueueAsync(cancelProjectJobDescription, BackgroundJobPriority.Normal,
            TimeSpan.FromSeconds(delay));
        _logger.LogInformation(
            "CancelProjectJobDescriptionEnd Job:{cancelProjectJobDescription} Expect execution time: {executionTime} jobId={jobId}", 
            cancelProjectJobDescription, executionTime, jobId);
    }

    private void LogWarning(string chainName, string projectId, int currentPeriod, int totalPeriod, long delay)
    {
        _logger.LogWarning(
            $"InvalidJob delay: {delay}. chain name: {chainName} project ID: {projectId}, current period: {currentPeriod}, total period: {totalPeriod}");
    }

    private void LogNewJob(ReleaseProjectTokenJobDescription jobDescription, long delay)
    {
        var executionTime = DateTimeOffset.UtcNow.AddSeconds(delay);
        _logger.LogInformation(
            $"Add New ReleaseProjectTokenJob Job:\n{jobDescription}\nExpect execution time: {executionTime}");
    }
}