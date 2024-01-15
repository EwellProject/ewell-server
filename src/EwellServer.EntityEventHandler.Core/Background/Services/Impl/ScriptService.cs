using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Contracts.Ewell;
using AElf.Types;
using EwellServer.EntityEventHandler.Core.Background.BackgroundJobs.BackgroundJobDescriptions;
using EwellServer.EntityEventHandler.Core.Background.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace EwellServer.EntityEventHandler.Core.Background.Services.Impl;

public class ScriptService : IScriptService, ITransientDependency
{
    private readonly IJobEnqueueService _jobEnqueueService;
    private readonly ITransactionService _transactionService;
    private readonly List<EwellInfo> _ewellInfoList;
    private readonly ILogger<ScriptService> _logger;

    public ScriptService(IJobEnqueueService jobEnqueueService, ITransactionService transactionService,
        IOptionsSnapshot<EwellOption> ewellOptionsSnapshot, ILogger<ScriptService> logger)
    {
        _jobEnqueueService = jobEnqueueService;
        _transactionService = transactionService;
        _logger = logger;
        _ewellInfoList = ewellOptionsSnapshot.Value.EwellInfoList.ToList();
    }

    public async Task<int> ProcessReleaseTokenAsync(ReleaseProjectTokenJobDescription releaseProjectTokenJobDescription)
    {
        var chainName = releaseProjectTokenJobDescription.ChainName;
        var projectId = releaseProjectTokenJobDescription.Id;
        var currentPeriod = releaseProjectTokenJobDescription.CurrentPeriod;
        _logger.LogInformation("ProcessReleaseTokenAsync Id={projectId} ChainName={chainName}", chainName, projectId);
        if (releaseProjectTokenJobDescription.IsNeedUnlockLiquidity)
        {
            // await LockLiquidityAsync(chainName, projectId, currentPeriod);
            await WithdrawAsync(chainName, projectId, currentPeriod);
        }

        await NextPeriodAsync(chainName, projectId, currentPeriod);
        var nextPeriod = currentPeriod + 1;
        return nextPeriod;
    }

    public async Task ProcessCancelProjectAsync(CancelProjectJobDescription cancelProjectJobDescription)
    {
        var chainName = cancelProjectJobDescription.ChainName;
        var projectId = cancelProjectJobDescription.Id;
        if (cancelProjectJobDescription.Users.Any())
        { 
            await RefundAllAsync(chainName, projectId, cancelProjectJobDescription.Users);
        }
        await ClaimLiquidatedDamageAllAsync(chainName, projectId);
    }

    private async Task LockLiquidityAsync(string chainName, string projectId, int period)
    {
        var ewellInfo = _ewellInfoList.First(x => x.ChainName == chainName);
        var txId = await _transactionService.SendTransactionAsync(chainName, ewellInfo.AdminKey, ewellInfo.EwellAddress,
            EwellConstants.LockLiquidity, Hash.LoadFromHex(projectId));
        await AddTransactionQueryJobAsync(chainName, projectId, EwellConstants.LockLiquidity, txId, period);
    }

    private async Task NextPeriodAsync(string chainName, string projectId, int currentPeriod)
    {
        var ewellInfo = _ewellInfoList.First(x => x.ChainName == chainName);
        var txId = await _transactionService.SendTransactionAsync(chainName, ewellInfo.AdminKey, ewellInfo.EwellAddress,
            EwellConstants.NextPeriod, Hash.LoadFromHex(projectId));
        await AddTransactionQueryJobAsync(chainName, projectId, EwellConstants.NextPeriod, txId, currentPeriod);
    }

    private async Task WithdrawAsync(string chainName, string projectId, int currentPeriod)
    {
        var ewellInfo = _ewellInfoList.First(x => x.ChainName == chainName);
        var txId = await _transactionService.SendTransactionAsync(chainName, ewellInfo.AdminKey, ewellInfo.EwellAddress,
            EwellConstants.Withdraw, Hash.LoadFromHex(projectId));
        await AddTransactionQueryJobAsync(chainName, projectId, EwellConstants.Withdraw, txId, currentPeriod);
    }

    private async Task RefundAllAsync(string chainName, string projectId, IReadOnlyCollection<string> users)
    {
        var ewellInfo = _ewellInfoList.First(x => x.ChainName == chainName);
        var input = new ReFundAllInput
        {
            ProjectId = Hash.LoadFromHex(projectId),
            Users = { users.Select(Address.FromBase58) }
        };
        var txId = await _transactionService.SendTransactionAsync(chainName, ewellInfo.AdminKey, ewellInfo.EwellAddress,
            EwellConstants.RefundAll, input);
        await AddTransactionQueryJobAsync(chainName, projectId, EwellConstants.RefundAll, txId);
    }

    private async Task ClaimLiquidatedDamageAllAsync(string chainName, string projectId)
    {
        var ewellInfo = _ewellInfoList.First(x => x.ChainName == chainName);
        var txId = await _transactionService.SendTransactionAsync(chainName, ewellInfo.AdminKey, ewellInfo.EwellAddress,
            EwellConstants.ClaimLiquidatedDamageAll, Hash.LoadFromHex(projectId));
        await AddTransactionQueryJobAsync(chainName, projectId, EwellConstants.ClaimLiquidatedDamageAll, txId);
    }

    private async Task AddTransactionQueryJobAsync(string chainName, string projectId, string opType, string txId,
        int period = 0)
    {
        var txQueryInfo = new QueryTransactionStatusJobDescription
        {
            ChainName = chainName,
            Id = projectId,
            Operation = opType,
            TransactionId = txId,
            CurrentPeriod = period
        };

        await _jobEnqueueService.AddJobAsync(txQueryInfo);
    }
}