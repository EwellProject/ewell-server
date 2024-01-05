using System.Collections.Generic;
using System.Threading.Tasks;
using EwellServer.Common.GraphQL;
using EwellServer.Dtos;
using EwellServer.Entities;
using GraphQL;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.DependencyInjection;

namespace EwellServer.User.Provider;

public interface IUserRecordGraphQLProvider
{
    Task<List<UserRecordIndex>> GetUserRecordListAsync(long startBlockHeight, string chainId, int maxResultCount, int skipCount);
}

public class UserRecordGraphQlProvider : IUserRecordGraphQLProvider, ISingletonDependency
{
    private readonly IGraphQlHelper _graphQlHelper;

    public UserRecordGraphQlProvider(IGraphQlHelper graphQlHelper)
    {
        _graphQlHelper = graphQlHelper;
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