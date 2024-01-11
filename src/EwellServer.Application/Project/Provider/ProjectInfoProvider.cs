using EwellServer.Common.GraphQL;
using EwellServer.Dtos;
using GraphQL;
using Volo.Abp.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.CSharp.Core;
using AElf.Indexing.Elasticsearch;
using EwellServer.Common;
using EwellServer.Entities;
using EwellServer.Project.Dto;
using Nest;

namespace EwellServer.Project.Provider;

public interface IProjectInfoProvider
{
    public Task<Tuple<long, List<CrowdfundingProjectIndex>>> GetProjectInfosAsync(QueryProjectInfoInput input,
        DateTime currentTime, string userAddress, List<string> userProjectIds);

    Task<List<CrowdfundingProjectIndex>> GetProjectListAsync(long startBlockHeight, string chainId, int maxResultCount,
        int skipCount);
}

public class ProjectInfoProvider : IProjectInfoProvider, ISingletonDependency
{
    private readonly IGraphQlHelper _graphQlHelper;
    private readonly INESTRepository<CrowdfundingProjectIndex, string> _crowdfundingProjectIndexRepository;

    public ProjectInfoProvider(INESTRepository<CrowdfundingProjectIndex, string> crowdfundingProjectIndexRepository,
        IGraphQlHelper graphQlHelper)
    {
        _crowdfundingProjectIndexRepository = crowdfundingProjectIndexRepository;
        _graphQlHelper = graphQlHelper;
    }

    public async Task<Tuple<long, List<CrowdfundingProjectIndex>>> GetProjectInfosAsync(QueryProjectInfoInput input,
        DateTime currentTime, string userAddress, List<string> userProjectIds)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CrowdfundingProjectIndex>, QueryContainer>>();

        var shouldQuery = new List<Func<QueryContainerDescriptor<CrowdfundingProjectIndex>, QueryContainer>>();

        if (!input.ChainId.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(i =>
                i.Field(f => f.ChainId).Value(input.ChainId)));
        }

        if (!input.ProjectId.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(i =>
                i.Field(f => f.Id).Value(input.ProjectId)));
        }
        
        AssemblyStatusQuery(input.Status, mustQuery, currentTime);
        AssemblyProjectTypesQuery(input.Types, shouldQuery, currentTime, userAddress, userProjectIds);
        
        if (shouldQuery.Any())
        {
            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }
        
        QueryContainer Filter(QueryContainerDescriptor<CrowdfundingProjectIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        //add sorting
        return await _crowdfundingProjectIndexRepository.GetListAsync(Filter, skip: input.SkipCount,
            limit: input.MaxResultCount,
            sortType: SortOrder.Descending,
            sortExp: p => p.StartTime);
    }

    private static void AssemblyStatusQuery(ProjectStatus status, 
        List<Func<QueryContainerDescriptor<CrowdfundingProjectIndex>, QueryContainer>> mustQuery,  DateTime current)
    {
        string currentStr = current.ToString("O");
        switch (status)
        {
            case ProjectStatus.All:
                break;
            case ProjectStatus.AboutToStart:
                mustQuery.Add(q => q.TermRange(i
                    => i.Field(index => index.StartTime.ToUtcMilliSeconds())
                        .GreaterThan(currentStr)));
                break;
            case ProjectStatus.Fundraising:
                mustQuery.Add(q => q.TermRange(i
                    => i.Field(index => index.StartTime.ToUtcMilliSeconds())
                        .LessThanOrEquals(currentStr)));
                mustQuery.Add(q => q.TermRange(i
                    => i.Field(index => index.EndTime.ToUtcMilliSeconds())
                        .GreaterThan(currentStr)));
                break;
            case ProjectStatus.WaitingUnlocked:
                mustQuery.Add(q => q.TermRange(i
                    => i.Field(index => index.EndTime.ToUtcMilliSeconds())
                        .LessThanOrEquals(currentStr)));
                mustQuery.Add(q => q.TermRange(i
                    => i.Field(index =>
                            index.TokenReleaseTime.AddSeconds(index.PeriodDuration.Mul(index.TotalPeriod))
                                .ToUtcMilliSeconds())
                        .GreaterThan(currentStr)));
                break;
            case ProjectStatus.Canceled:
                mustQuery.Add(q => q.Term(i =>
                    i.Field(f => f.IsCanceled).Value(true)));
                break;
            case ProjectStatus.Ended:
                var shouldQuery = new List<Func<QueryContainerDescriptor<CrowdfundingProjectIndex>, QueryContainer>>();
                //add or query
                shouldQuery.Add(q => q.Term(i =>
                    i.Field(f => f.IsCanceled).Value(true)));
                shouldQuery.Add(q => q.TermRange(i
                    => i.Field(index => index.TokenReleaseTime.AddSeconds(index.PeriodDuration.Mul(index.TotalPeriod))
                            .ToUtcMilliSeconds())
                        .LessThanOrEquals(currentStr)));
                mustQuery.Add(m => m.Bool(mb => mb.Should(shouldQuery)));
                break;
        }
    }

    private static void AssemblyProjectTypesQuery(List<ProjectType> types,
        List<Func<QueryContainerDescriptor<CrowdfundingProjectIndex>, QueryContainer>> shouldQuery, 
        DateTime current, string userAddress, List<string> userProjectIds)
    {
        if (types.IsNullOrEmpty())
        {
            return;
        }

        foreach (var projectType in types)
        {
            switch (projectType)
            {
                case ProjectType.Active:
                    var aboutToStartMustQuery =
                        new List<Func<QueryContainerDescriptor<CrowdfundingProjectIndex>, QueryContainer>>();
                    AssemblyStatusQuery(ProjectStatus.AboutToStart, aboutToStartMustQuery, current);

                    var fundraisingMustQuery =
                        new List<Func<QueryContainerDescriptor<CrowdfundingProjectIndex>, QueryContainer>>();
                    AssemblyStatusQuery(ProjectStatus.Fundraising, fundraisingMustQuery, current);

                    var waitingUnlockedMustQuery =
                        new List<Func<QueryContainerDescriptor<CrowdfundingProjectIndex>, QueryContainer>>();
                    AssemblyStatusQuery(ProjectStatus.WaitingUnlocked, waitingUnlockedMustQuery, current);

                    shouldQuery.Add(s => s.Bool(sb => sb.Must(aboutToStartMustQuery)));
                    shouldQuery.Add(s => s.Bool(sb => sb.Must(fundraisingMustQuery)));
                    shouldQuery.Add(s => s.Bool(sb => sb.Must(waitingUnlockedMustQuery)));
                    break;
                case ProjectType.Closed:
                    var endedMustQuery =
                        new List<Func<QueryContainerDescriptor<CrowdfundingProjectIndex>, QueryContainer>>();
                    AssemblyStatusQuery(ProjectStatus.Ended, endedMustQuery, current);
                    shouldQuery.Add(s => s.Bool(sb => sb.Must(endedMustQuery)));
                    break;
                case ProjectType.Created:
                    if (!userAddress.IsNullOrEmpty())
                    {
                        shouldQuery.Add(q => q.Term(i =>
                            i.Field(f => f.Creator).Value(userAddress)));
                    }

                    break;
                case ProjectType.Participate:
                    if (!userProjectIds.IsNullOrEmpty())
                    {
                        shouldQuery.Add(q => q.Terms(i =>
                            i.Field(f => f.Id).Terms(userProjectIds)));
                    }
                    break;
            }
        }
    }

    public async Task<List<CrowdfundingProjectIndex>> GetProjectListAsync(long startBlockHeight, string chainId,
        int maxResultCount, int skipCount)
    {
        var response = await _graphQlHelper.QueryAsync<CrowdfundingProjectPageResult>(new GraphQLRequest
        {
            Query = @"
			    query ($chainId:String,$startBlockHeight:Long!,$maxResultCount:Int,$skipCount:Int) {
                    getProjectList(input: {$chainId:$chainId,$startBlockHeight:$startBlockHeight,$maxResultCount:$maxResultCount,$skipCount:$skipCount}){
                        data{
                                id,chainId,blockHeight,creator,behaviorType,crowdFundingType,startTime,endTime,tokenReleaseTime,
                                toRaisedAmount,crowdFundingIssueAmount,preSalePrice,publicSalePrice,minSubscription,maxSubscription,listMarketInfo,
                                liquidityLockProportion,unlockTime,firstDistributeProportion,restDistributeProportion,totalPeriod,additionalInfo,isCanceled
                                wsEnableWhitelist,whitelistId,currentRaisedAmount,currentCrowdFundingIssueAmount,participantCount,chainId,currentPeriod
                                periodDuration,isBurnRestToken,receivableLiquidatedDamageAmount,lastModificationTime
                                toRaiseToken{symbol,name,address,decimals},
                                crowdFundingIssueToken{symbol,name,address,decimals}
                            }
                        ,totalCount
                    }
                }",
            Variables = new
            {
                startBlockHeight, chainId, maxResultCount, skipCount
            }
        });
        return response.Data.IsNullOrEmpty() ? new List<CrowdfundingProjectIndex>() : response.Data;
    }
}