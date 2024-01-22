using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using EwellServer.Common.GraphQL;
using EwellServer.Dtos;
using EwellServer.Entities;
using EwellServer.Project.Index;
using GraphQL;
using Microsoft.IdentityModel.Tokens;
using Nest;
using Volo.Abp.DependencyInjection;

namespace EwellServer.Project.Provider;

public interface IUserProjectInfoProvider
{
    Task<Dictionary<string, UserProjectInfoIndex>> GetUserProjectInfosAsync(string user);

    Task<List<UserProjectInfoIndex>> GetSyncUserProjectInfosAsync(int skipCount, string chainId, long startBlockHeight, long endBlockHeight);
    
    Task<List<CrowdfundingProjectIndex>> GetProjectListAsync(long startBlockHeight, long endBlockHeight, string chainId, int maxResultCount, int skipCount);
    
    Task<List<UserRecordIndex>> GetUserRecordListAsync(long startBlockHeight, long endBlockHeight, string chainId, int maxResultCount, int skipCount);
    Task<List<Whitelist>> GetWhitelistListAsync(long startBlockHeight, long endBlockHeight, string chainId, int maxResultCount, int skipCount);

    Task<Tuple<long, List<UserProjectInfoIndex>>> GetProjectUserListAsync(string projectId, string chainId, string address, int maxResultCount, int skipCount);
}

public class UserProjectInfoProvider : IUserProjectInfoProvider, ISingletonDependency
{
    private readonly IGraphQlHelper _graphQlHelper;
    private readonly INESTRepository<UserProjectInfoIndex, string> _userProjectInfoIndexRepository;
    
    public UserProjectInfoProvider(IGraphQlHelper graphQlHelper, 
        INESTRepository<UserProjectInfoIndex, string> userProjectInfoIndexRepository)
    {
        _graphQlHelper = graphQlHelper;
        _userProjectInfoIndexRepository = userProjectInfoIndexRepository;
    }

    public async Task<Dictionary<string, UserProjectInfoIndex>> GetUserProjectInfosAsync(string user)
    {
        if (user.IsNullOrWhiteSpace())
        {
            return new Dictionary<string, UserProjectInfoIndex>();
        }

        var mustQuery = new List<Func<QueryContainerDescriptor<UserProjectInfoIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i =>
            i.Field(f => f.User).Value(user)));

        QueryContainer Filter(QueryContainerDescriptor<UserProjectInfoIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var tuple = await _userProjectInfoIndexRepository.GetListAsync(Filter);

        return !tuple.Item2.IsNullOrEmpty()
            ? tuple.Item2.ToDictionary(item => item.CrowdfundingProjectId, item => item)
            : new Dictionary<string, UserProjectInfoIndex>();
    }
    
    public async Task<Tuple<long, List<UserProjectInfoIndex>>> GetProjectUserListAsync(string projectId, string chainId, string address, int maxResultCount, int skipCount)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UserProjectInfoIndex>, QueryContainer>>
        {
            q => q.Term(i => i.Field(f => f.ChainId).Value(chainId)),
            q => q.Term(i => i.Field(f => f.Id).Value(projectId))
        };
        if (!address.IsNullOrEmpty())
        {
            if (address.Contains("_"))
            {
                address = address.Split("_")[1];
            }
            mustQuery.Add(q => q.Term(i => i.Field(f => f.User).Value(address)));
        }
        QueryContainer Filter(QueryContainerDescriptor<UserProjectInfoIndex> f) => f.Bool(b => b.Must(mustQuery));
        
        return await _userProjectInfoIndexRepository.GetSortListAsync(Filter, skip: skipCount, limit: maxResultCount, 
            sortFunc: _ => new SortDescriptor<UserProjectInfoIndex>().Descending(index => index.InvestAmount));
    }

    public async Task<List<UserProjectInfoIndex>> GetSyncUserProjectInfosAsync(int skipCount, string chainId, long startBlockHeight, long endBlockHeight)
    {
        var graphQlResponse = await _graphQlHelper.QueryAsync<IndexerUserProjectInfoSync>(new GraphQLRequest
        {
            Query =
                @"query($skipCount:Int!,$chainId:String,$startBlockHeight:Long!,$endBlockHeight:Long!){
            dataList:getSyncUserProjectInfos(input: {skipCount:$skipCount,chainId:$chainId,startBlockHeight:$startBlockHeight,endBlockHeight:$endBlockHeight})
            {
                id,chainId,blockHeight,createTime
                user,crowdfundingProjectId,investAmount,toClaimAmount,actualClaimAmount,
                crowdfundingProject{id,chainId,blockHeight,creator,crowdFundingType,startTime,endTime,tokenReleaseTime}
            }}",
            Variables = new
            {
                skipCount,
                chainId,
                startBlockHeight,
                endBlockHeight
            }
        });
        return graphQlResponse?.DataList ?? new List<UserProjectInfoIndex>();
    }
    
    public async Task<List<CrowdfundingProjectIndex>> GetProjectListAsync(long startBlockHeight, long endBlockHeight, string chainId, int maxResultCount, int skipCount)
    {
        var response =  await _graphQlHelper.QueryAsync<CrowdfundingProjectCommonResult>(new GraphQLRequest
        {
            Query = @"
			    query ($chainId:String!,$startBlockHeight:Long!,$endBlockHeight:Long!,$maxResultCount:Int!,$skipCount:Int!) {
                    data:getProjectList(input: {chainId:$chainId,startBlockHeight:$startBlockHeight,endBlockHeight:$endBlockHeight,maxResultCount:$maxResultCount,skipCount:$skipCount}){
                        data{
                                id,chainId,blockHeight,creator,crowdFundingType,startTime,endTime,tokenReleaseTime,createTime,cancelTime,
                                toRaisedAmount,crowdFundingIssueAmount,preSalePrice,publicSalePrice,minSubscription,maxSubscription,listMarketInfo,
                                liquidityLockProportion,unlockTime,firstDistributeProportion,restDistributeProportion,totalPeriod,additionalInfo,isCanceled,
                                isEnableWhitelist,whitelistId,currentRaisedAmount,currentCrowdFundingIssueAmount,participantCount,chainId,currentPeriod,
                                periodDuration,isBurnRestToken,receivableLiquidatedDamageAmount,lastModificationTime,
                                toRaiseToken{chainId,symbol},
                                crowdFundingIssueToken{chainId,symbol}
                            }
                        ,totalCount
                    }
                }",
            Variables = new
            {
                chainId, startBlockHeight, endBlockHeight, maxResultCount, skipCount
            }
        });
        return response.Data?.Data ?? new List<CrowdfundingProjectIndex>();
    }
    
    public async Task<List<UserRecordIndex>> GetUserRecordListAsync(long startBlockHeight, long endBlockHeight, string chainId, int maxResultCount, int skipCount)
    {
        var response =  await _graphQlHelper.QueryAsync<UserRecordCommonResult>(new GraphQLRequest
        {
            Query = @"
			    query ($chainId:String!,$startBlockHeight:Long!,$endBlockHeight:Long!,$maxResultCount:Int!,$skipCount:Int!) {
                    data:getUserRecordList(input: {chainId:$chainId,startBlockHeight:$startBlockHeight,endBlockHeight:$endBlockHeight,maxResultCount:$maxResultCount,skipCount:$skipCount}){
                        data{
                                id,chainId,user,behaviorType,toRaiseTokenAmount,crowdFundingIssueAmount,dateTime,blockHeight
                                crowdfundingProject{chainId,blockHeight,id,creator,crowdFundingType,startTime,endTime,tokenReleaseTime},
                                toRaiseToken{symbol},
                                crowdFundingIssueToken{symbol}
                            }
                        ,totalCount
                    }
                }",
            Variables = new
            {
                chainId, startBlockHeight, endBlockHeight, maxResultCount, skipCount
            }
        });
        return response.Data?.Data ?? new List<UserRecordIndex>();
    }

    public async Task<List<Whitelist>> GetWhitelistListAsync(long startBlockHeight, long endBlockHeight, string chainId, int maxResultCount, int skipCount)
    {
        var response =  await _graphQlHelper.QueryAsync<WhitelistCommonResult>(new GraphQLRequest
        {
            Query = @"
			    query ($chainId:String!,$startBlockHeight:Long!,$endBlockHeight:Long!,$maxResultCount:Int!,$skipCount:Int!) {
                    data:getWhitelistList(input: {chainId:$chainId,startBlockHeight:$startBlockHeight,endBlockHeight:$endBlockHeight,maxResultCount:$maxResultCount,skipCount:$skipCount}){
                        data{
                                id,chainId,blockHeight,isAvailable
                            }
                        ,totalCount
                    }
                }",
            Variables = new
            {
                chainId, startBlockHeight, endBlockHeight, maxResultCount, skipCount
            }
        });
        return response.Data?.Data ?? new List<Whitelist>();
    }
}