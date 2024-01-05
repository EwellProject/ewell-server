using System.Collections.Generic;
using System.Threading.Tasks;
using EwellServer.Common.GraphQL;
using EwellServer.Dtos;
using EwellServer.Entities;
using GraphQL;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.DependencyInjection;

namespace EwellServer.GraphQL;

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
                                chainId,id,blockHeight,
                                user,behaviorType,toRaiseTokenAmount,crowdFundingIssueAmount,dateTime,
                                crowdfundingProjectId,
                                crowdfundingProjectBase{id,creator,crowdFundingType,startTime,endTime,tokenReleaseTime}
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