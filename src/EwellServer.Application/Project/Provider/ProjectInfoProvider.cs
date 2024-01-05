using System.Collections.Generic;
using System.Threading.Tasks;
using EwellServer.Common.GraphQL;
using EwellServer.Dtos;
using EwellServer.Entities;
using GraphQL;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.DependencyInjection;

namespace EwellServer.Project.Provider;

public interface IProjectInfoProvider
{
    Task<List<CrowdfundingProjectIndex>> GetProjectListAsync(long startBlockHeight, string chainId, int maxResultCount, int skipCount);
}

public class ProjectInfoProvider : IProjectInfoProvider, ISingletonDependency
{
    private readonly IGraphQlHelper _graphQlHelper;

    public ProjectInfoProvider(IGraphQlHelper graphQlHelper)
    {
        _graphQlHelper = graphQlHelper;
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
        return CollectionUtilities.IsNullOrEmpty(response.Data) ? [] : response.Data;
    }
}