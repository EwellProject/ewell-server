using System.Collections.Generic;
using System.Threading.Tasks;
using EwellServer.Common.GraphQL;
using EwellServer.Entities;
using EwellServer.Project.Index;
using GraphQL;
using Volo.Abp.DependencyInjection;

namespace EwellServer.Project.Provider;

public interface IUserProjectInfoProvider
{
    Task<List<UserProjectInfoIndex>> GetSyncUserProjectInfosAsync(string chainId, long startBlockHeight, long endBlockHeight);
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
}