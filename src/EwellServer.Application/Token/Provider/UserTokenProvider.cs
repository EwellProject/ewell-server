using System.Collections.Generic;
using System.Threading.Tasks;
using EwellServer.Common.GraphQL;
using EwellServer.Token.Index;
using GraphQL;
using Volo.Abp.DependencyInjection;

namespace EwellServer.Token.Provider;

public interface IUserTokenProvider
{
    Task<List<IndexerUserToken>> GetUserTokens(string chainId, string address);
}

public class UserTokenProvider : IUserTokenProvider, ISingletonDependency
{
    private readonly IGraphQlHelper _graphQlHelper;

    public UserTokenProvider(IGraphQlHelper graphQlHelper)
    {
        _graphQlHelper = graphQlHelper;
    }

    public async Task<List<IndexerUserToken>> GetUserTokens(string chainId, string address)
    {
        var response =  await _graphQlHelper.QueryAsync<List<IndexerUserToken>>(new GraphQLRequest
        {
            Query = @"
			    query ($chainId:String!,$address:Long!) {
                    data:getUserTokenInfos(input: {chainId:$chainId,address:$address}){
                        data{
                           chainId,symbol,tokenName,imageUrl,decimals,balance
                        }
                    }
                }",
            Variables = new
            {
                chainId, address
            }
        });
        return response?? new List<IndexerUserToken>();
    }
}