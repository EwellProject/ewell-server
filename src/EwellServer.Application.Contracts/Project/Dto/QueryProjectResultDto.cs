using System;
using System.Collections.Generic;
using System.Linq;
using AElf.CSharp.Core;
using EwellServer.Entities;
using EwellServer.Token;

namespace EwellServer.Project.Dto;

public class QueryProjectResultDto
{
    public long TotalCount { get; set; } = 0;
    
    public QueryProjectResultBaseDto Detail { get; set; }
    
    public List<QueryProjectResultBaseDto> ActiveItems { get; set; } = new();
    
    public List<QueryProjectResultBaseDto> ClosedItems { get; set; } = new();
    
    public List<QueryProjectResultBaseDto> CreatedItems { get; set; } = new();
    
    public List<QueryProjectResultBaseDto> ParticipateItems { get; set; } = new();


    public void OfResultDto(string user, string projectId, List<ProjectType> types, 
        QueryProjectResultBaseDto resultBase, Dictionary<string, UserProjectInfoIndex> userProjectDict)
    {
        //only set Detail
        if (!projectId.IsNullOrEmpty() && types.IsNullOrEmpty())
        {
            Detail = resultBase;
            return;
        }
        TypesHandle(user, types, resultBase, userProjectDict);
    }

    private void TypesHandle(string user, List<ProjectType> types, QueryProjectResultBaseDto resultBase, 
        Dictionary<string, UserProjectInfoIndex> userProjectDict)
    {
        if (types.Contains(ProjectType.Created))
        {
            if (resultBase.Creator.Equals(user))
            {
                CreatedItems.Add(resultBase);
            }
        }
    
        if (types.Contains(ProjectType.Participate))
        {
            if (userProjectDict.TryGetValue(resultBase.Id, out var _))
            {
                ParticipateItems.Add(resultBase);
            }
        }
    
        if (types.Contains(ProjectType.Active))
        {
            if (resultBase.Status is ProjectStatus.AboutToStart or ProjectStatus.Fundraising
                or ProjectStatus.WaitingUnlocked)
            {
                ActiveItems.Add(resultBase);
            }
        }
    
        if (types.Contains(ProjectType.Closed))
        {
            if (resultBase.IsCanceled || resultBase.Status == ProjectStatus.Ended)
            {
                ClosedItems.Add(resultBase);
            }
        }
        
    }

    public void AddSorting()
    {
        ActiveItems = ActiveItems.OrderByDescending(item => item.CurrentRaisedAmount).ToList();
        ClosedItems = ClosedItems.OrderByDescending(item => item.RealEndTime)
            .ThenByDescending(item => item.CancelTime)
            .ToList();
        CreatedItems = CreatedItems.OrderByDescending(item => item.CreateTime).ToList();
        ParticipateItems = ParticipateItems
            .OrderBy(item => item.Status)
            .ThenByDescending(item => item.InvestCreateTime).ToList();
    }
}

public class QueryProjectResultBaseDto
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
    public DateTime InvestCreateTime { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? CancelTime { get; set; }

    public ProjectStatus Status  { get; set; }
    public DateTime RealEndTime  { get; set; }
    
    public void OfResultBase(string userAddress, DateTime current, Dictionary<string, UserProjectInfoIndex> userProjectDict)
    {
        //convert status
        if (IsCanceled)
        {
            Status = ProjectStatus.Canceled;
        }
        else if (current < StartTime)
        {
            Status = ProjectStatus.AboutToStart;
        }
        else if (current < EndTime)
        {
            Status = ProjectStatus.Fundraising;
        }
        else if (current < RealEndTime)
        {
            Status = ProjectStatus.WaitingUnlocked;
        }
        else
        {
            Status = ProjectStatus.Ended;
        }
        
        if (!userAddress.IsNullOrEmpty() && userProjectDict.TryGetValue(Id, out var userProject))
        {
            InvestAmount = userProject.InvestAmount;
            ToClaimAmount = userProject.ToClaimAmount;
            InvestCreateTime = userProject.CreateTime;
        }
    }
}