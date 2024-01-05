using System;
using System.Collections.Generic;
using EwellServer.Token;

namespace EwellServer.Project.Dto;

public class QueryProjectResultDto
{
    private long TotalCount { get; set; } = 0;

    private List<QueryProjectResultBase> ActiveItems { get; set; } = new List<QueryProjectResultBase>();
    
    private List<QueryProjectResultBase> ClosedItems { get; set; } = new List<QueryProjectResultBase>();
    
    private List<QueryProjectResultBase> CreatedItems { get; set; } = new List<QueryProjectResultBase>();
    
    private List<QueryProjectResultBase> ParticipateItems { get; set; } = new List<QueryProjectResultBase>();
}

public class QueryProjectResultBase
{
    public string ChainId { get; set; }
    public long BlockHeight { get; set; }
    public string Id { get; set; }
    public string Creator { get; set; }
    public string CrowdFundingType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime TokenReleaseTime { get; set; }
    public long ToRaisedAmount { get; set; }
    public long CrowdFundingIssueAmount { get; set; }
    public long PreSalePrice { get; set; }
    public long PublicSalePrice { get; set; }
    public long MinSubscription { get; set; }
    public long MaxSubscription { get; set; }
    public string ListMarketInfo { get; set; }
    public int LiquidityLockProportion { get; set; }
    public DateTime? UnlockTime { get; set; }
    public int FirstDistributeProportion { get; set; }
    public int RestDistributeProportion { get; set; }
    public int TotalPeriod { get; set; }
    public string AdditionalInfo { get; set; }
    public bool IsCanceled { get; set; }
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
    public TokenBasicInfo ToRaiseToken { get; set; }
    public TokenBasicInfo CrowdFundingIssueToken { get; set; }
    public string User { get; set; }
    public long InvestAmount { get; set; }
    public long ToClaimAmount { get; set; }
}