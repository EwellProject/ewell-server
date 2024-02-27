using System;
using Nest;

namespace EwellServer.Project;

public class CrowdfundingProjectBasicProperty : CrowdfundingProjectBase
{
    public long TargetRaisedAmount { get; set; }
    public long CrowdFundingIssueAmount { get; set; }
    public long PreSalePrice { get; set; }
    public long PublicSalePrice { get; set; }
    public long MinSubscription { get; set; }
    public long MaxSubscription { get; set; }
    public string ListMarketInfo { get; set; }
    public int LiquidityLockProportion { get; set; }
    public DateTime? UnlockTime { get; set; }
    public int FirstDistributeProportion { get; set; }
    public int RestPeriodDistributeProportion { get; set; }
    public int TotalPeriod { get; set; }
    public string AdditionalInfo { get; set; }
    public bool IsCanceled { get; set; }
    public bool IsWithdraw { get; set; }
    public DateTime? CancelTime { get; set; }
    public DateTime? WithdrawTime { get; set; }
    public bool IsEnableWhitelist { get; set; }
    public string WhitelistId { get; set; }
    public long CurrentRaisedAmount { get; set; }
    public long CurrentCrowdFundingIssueAmount { get; set; }
    public long ParticipantCount { get; set; }
    public int CurrentPeriod { get; set; }
    public long PeriodDuration { get; set; }
    public bool IsBurnRestToken { get; set; }
    public long ReceivableLiquidatedDamageAmount { get; set; }
    public DateTime? LastModificationTime { get; set; }
    //TokenReleaseTime.AddSeconds(PeriodDuration.Mul(TotalPeriod)）
    public DateTime RealEndTime { get; set; }
    [Keyword] public string VirtualAddress { get; set; }
}