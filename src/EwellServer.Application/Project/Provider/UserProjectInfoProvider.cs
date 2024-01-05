using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using EwellServer.Common.GraphQL;
using EwellServer.Entities;
using EwellServer.Project.Index;
using GraphQL;
using Nest;
using Volo.Abp.DependencyInjection;

namespace EwellServer.Project.Provider;

public interface IUserProjectInfoProvider
{
    Task<Dictionary<string, UserProjectInfoIndex>> GetUserProjectInfosAsync(string user);

    Task<List<UserProjectInfoIndex>> GetSyncUserProjectInfosAsync(int skipCount, string chainId, long startBlockHeight, long endBlockHeight);
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
        var mustQuery = new List<Func<QueryContainerDescriptor<UserProjectInfoIndex>, QueryContainer>>();

        if (!user.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(i =>
                i.Field(f => f.User).Value(user)));
        }

        QueryContainer Filter(QueryContainerDescriptor<UserProjectInfoIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        //sortExp base Sort , like "floorPrice", "itemTotal"
        var tuple = await _userProjectInfoIndexRepository.GetListAsync(Filter);

        return !tuple.Item2.IsNullOrEmpty() 
            ? tuple.Item2.ToDictionary(item => item.CrowdfundingProjectId, item => item) 
            : new Dictionary<string, UserProjectInfoIndex>();
    }

    public async Task<List<UserProjectInfoIndex>> GetSyncUserProjectInfosAsync(int skipCount, string chainId, long startBlockHeight, long endBlockHeight)
    {
        var graphQlResponse = await _graphQlHelper.QueryAsync<IndexerUserProjectInfoSync>(new GraphQLRequest
        {
            Query =
                @"query($skipCount:Int!,$chainId:String,$startBlockHeight:Long!,$endBlockHeight:Long!){
            dataList:getSyncUserProjectInfos(dto: {skipCount:$skipCount,chainId:$chainId,startBlockHeight:$startBlockHeight,endBlockHeight:$endBlockHeight})
            {
                id,chainId,blockHeight
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
        return graphQlResponse.Data.DataList.IsNullOrEmpty() ? new List<UserProjectInfoIndex>() : graphQlResponse.Data.DataList;
    }
}