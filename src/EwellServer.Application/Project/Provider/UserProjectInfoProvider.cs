using System.Collections.Generic;
using System.Threading.Tasks;
using EwellServer.Common.GraphQL;
using EwellServer.Dtos;
using EwellServer.Entities;
using EwellServer.Project.Index;
using GraphQL;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.DependencyInjection;

namespace EwellServer.Project.Provider;

public interface IUserProjectInfoProvider
{
    Task<List<UserProjectInfoIndex>> GetSyncUserProjectInfosAsync(string chainId, long startBlockHeight, long endBlockHeight);
    Task<List<CrowdfundingProjectIndex>> GetProjectListAsync(long startBlockHeight, string chainId, int maxResultCount, int skipCount);
    Task<List<UserRecordIndex>> GetUserRecordListAsync(long startBlockHeight, string chainId, int maxResultCount, int skipCount);
}

public class UserProjectInfoProvider : IUserProjectInfoProvider, ISingletonDependency
{
    private readonly IGraphQlHelper _graphQlHelper;

    public UserProjectInfoProvider(IGraphQlHelper graphQlHelper)
    {
        _graphQlHelper = graphQlHelper;
    }

    public async Task<List<UserProjectInfoIndex>> GetSyncUserProjectInfosAsync(string chainId, long startBlockHeight, long endBlockHeight)
    {
        var graphQlResponse = await _graphQlHelper.QueryAsync<IndexerUserProjectInfoSync>(new GraphQLRequest
        {
            Query =
                @"query($chainId:String,$startBlockHeight:Long!,$endBlockHeight:Long!){
            dataList:getSyncUserProjectInfos(dto: {chainId:$chainId,startBlockHeight:$startBlockHeight,endBlockHeight:$endBlockHeight})
            {
                id,chainId,blockHeight
                user,crowdfundingProjectId,investAmount,toClaimAmount,actualClaimAmount,
                crowdfundingProject{id,chainId,blockHeight,creator,crowdFundingType,startTime,endTime,tokenReleaseTime}
            }}",
            Variables = new
            {
                chainId,
                startBlockHeight,
                endBlockHeight
            }
        });
        return graphQlResponse.Data.DataList.IsNullOrEmpty() ? new List<UserProjectInfoIndex>() : graphQlResponse.Data.DataList;
    }
    
    public async Task<List<CrowdfundingProjectIndex>> GetProjectListAsync(long startBlockHeight, string chainId, int maxResultCount, int skipCount)
    {
        var response =  await _graphQlHelper.QueryAsync<CrowdfundingProjectPageResult>(new GraphQLRequest
        {
            Query = @"
			    query ($chainId:String,$startBlockHeight:Long!,$maxResultCount:Int,$skipCount:Int) {
                    getProjectList(dto: {$chainId:$chainId,$startBlockHeight:$startBlockHeight,$maxResultCount:$maxResultCount,$skipCount:$skipCount}){
                        data{
                                id,chainId,blockHeight,creator,behaviorType,crowdFundingType,startTime,endTime,tokenReleaseTime,
                                toRaisedAmount,crowdFundingIssueAmount,preSalePrice,publicSalePrice,minSubscription,maxSubscription,listMarketInfo,
                                liquidityLockProportion,unlockTime,firstDistributeProportion,restDistributeProportion,totalPeriod,additionalInfo,isCanceled
                                wsEnableWhitelist,whitelistId,currentRaisedAmount,currentCrowdFundingIssueAmount,participantCount,chainId,currentPeriod
                                periodDuration,isBurnRestToken,receivableLiquidatedDamageAmount,lastModificationTime
                                toRaiseToken{symbol},
                                crowdFundingIssueToken{symbol}
                            }
                        ,totalCount
                    }
                }",
            Variables = new
            {
                startBlockHeight, chainId, maxResultCount, skipCount
            }
        });
        return CollectionUtilities.IsNullOrEmpty(response.Data) ? new List<CrowdfundingProjectIndex>() : response.Data;
    }
    
    public async Task<List<UserRecordIndex>> GetUserRecordListAsync(long startBlockHeight, string chainId, int maxResultCount, int skipCount)
    {
        var response =  await _graphQlHelper.QueryAsync<UserRecordPageResult>(new GraphQLRequest
        {
            Query = @"
			    query ($chainId:String,$startBlockHeight:Long!,$maxResultCount:Int,$skipCount:Int) {
                    getUserRecordList(dto: {$chainId:$chainId,$startBlockHeight:$startBlockHeight,$maxResultCount:$maxResultCount,$skipCount:$skipCount}){
                        data{
                                id,chainId,user,behaviorType,toRaiseTokenAmount,crowdFundingIssueAmount,dateTime,blockHeight
                                crowdfundingProjectBase{chainId,blockHeight,id,creator,crowdFundingType,startTime,endTime,tokenReleaseTime},
                                toRaiseToken{symbol},
                                crowdFundingIssueToken{symbol}
                            }
                        ,totalCount
                    }
                }",
            Variables = new
            {
                startBlockHeight, chainId, maxResultCount, skipCount
            }
        });
        return CollectionUtilities.IsNullOrEmpty(response.Data) ? new List<UserRecordIndex>() : response.Data;
    }
}