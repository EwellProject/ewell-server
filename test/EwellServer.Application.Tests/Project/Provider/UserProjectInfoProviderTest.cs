using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using EwellServer.Common.GraphQL;
using EwellServer.Dtos;
using EwellServer.Entities;
using EwellServer.Project.Index;
using GraphQL;
using Nest;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EwellServer.Project.Provider;

public class UserProjectInfoProviderTest
{
    private readonly IGraphQlHelper _graphQlHelper;
    private readonly INESTRepository<UserProjectInfoIndex, string> _userProjectInfoIndexRepository;
    private readonly IUserProjectInfoProvider _provider;

    public UserProjectInfoProviderTest()
    {
        _graphQlHelper = Substitute.For<IGraphQlHelper>();
        _userProjectInfoIndexRepository = Substitute.For<INESTRepository<UserProjectInfoIndex, string>>();
        _provider = new UserProjectInfoProvider(_graphQlHelper, _userProjectInfoIndexRepository);
    }
    
    [Fact]
    public async Task GetUserProjectInfosAsync_Test()
    {
        // first return
        var result = await _provider.GetUserProjectInfosAsync("");
        result.ShouldNotBeNull();
        
        // second return
        _userProjectInfoIndexRepository
            .GetListAsync(Arg.Any<Func<QueryContainerDescriptor<UserProjectInfoIndex>, QueryContainer>>())
            .Returns(Task.FromResult(new Tuple<long, List<UserProjectInfoIndex>>(1, new List<UserProjectInfoIndex>())));
        result = await _provider.GetUserProjectInfosAsync("user");
        result.ShouldNotBeNull();
        
        // third return
        _userProjectInfoIndexRepository
            .GetListAsync(Arg.Any<Func<QueryContainerDescriptor<UserProjectInfoIndex>, QueryContainer>>())
            .Returns(Task.FromResult(new Tuple<long, List<UserProjectInfoIndex>>(1, new List<UserProjectInfoIndex> { new() { CrowdfundingProjectId = "CrowdfundingProjectId" } })));
        result = await _provider.GetUserProjectInfosAsync("user");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetSyncUserProjectInfosAsync_Test()
    {
        _graphQlHelper.QueryAsync<IndexerUserProjectInfoSync>(Arg.Any<GraphQLRequest>())
            .Returns(Task.FromResult(new IndexerUserProjectInfoSync()));
        var result = await _provider.GetSyncUserProjectInfosAsync(0, "chainId", 1, 1);
        result.ShouldNotBeNull();
        
        _graphQlHelper.QueryAsync<IndexerUserProjectInfoSync>(Arg.Any<GraphQLRequest>())
            .Returns(Task.FromResult(new IndexerUserProjectInfoSync(){DataList = new List<UserProjectInfoIndex>()}));
        result = await _provider.GetSyncUserProjectInfosAsync(0, "chainId", 1, 1);
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task GetProjectListAsync_Test()
    {
        _graphQlHelper.QueryAsync<CrowdfundingProjectCommonResult>(Arg.Any<GraphQLRequest>())
            .Returns(Task.FromResult(new CrowdfundingProjectCommonResult()));
        var result = await _provider.GetProjectListAsync(1, 1, "chainId", 1, 1);
        result.ShouldNotBeNull();
        
        _graphQlHelper.QueryAsync<CrowdfundingProjectCommonResult>(Arg.Any<GraphQLRequest>())
            .Returns(Task.FromResult(new CrowdfundingProjectCommonResult {Data = new CrowdfundingProjectPageResult(1, new List<CrowdfundingProjectIndex>())}));
        result = await _provider.GetProjectListAsync(1, 1, "chainId", 1, 1);
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task GetUserRecordListAsync_Test()
    {
        _graphQlHelper.QueryAsync<UserRecordCommonResult>(Arg.Any<GraphQLRequest>())
            .Returns(Task.FromResult(new UserRecordCommonResult()));
        var result = await _provider.GetUserRecordListAsync(1, 1, "chainId", 1, 1);
        result.ShouldNotBeNull();
        
        _graphQlHelper.QueryAsync<UserRecordCommonResult>(Arg.Any<GraphQLRequest>())
            .Returns(Task.FromResult(new UserRecordCommonResult {Data = new UserRecordPageResult(1, new List<UserRecordIndex>())}));
        result = await _provider.GetUserRecordListAsync(1, 1, "chainId", 1, 1);
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task GetWhitelistListAsync_Test()
    {
        _graphQlHelper.QueryAsync<WhitelistCommonResult>(Arg.Any<GraphQLRequest>())
            .Returns(Task.FromResult(new WhitelistCommonResult()));
        var result = await _provider.GetWhitelistListAsync(1, 1, "chainId", 1, 1);
        result.ShouldNotBeNull();
        
        _graphQlHelper.QueryAsync<WhitelistCommonResult>(Arg.Any<GraphQLRequest>())
            .Returns(Task.FromResult(new WhitelistCommonResult {Data = new WhitelistPageResult(1, new List<Whitelist>())}));
        result = await _provider.GetWhitelistListAsync(1, 1, "chainId", 1, 1);
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task GetProjectUserListAsync_Test()
    {
        _userProjectInfoIndexRepository
            .GetSortListAsync(Arg.Any<Func<QueryContainerDescriptor<UserProjectInfoIndex>, QueryContainer>>(), null,
                Arg.Any<Func<SortDescriptor<UserProjectInfoIndex>, IPromise<IList<ISort>>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>())
            .Returns(Task.FromResult(new Tuple<long, List<UserProjectInfoIndex>>(1, new List<UserProjectInfoIndex>())));
        var result = await _provider.GetProjectUserListAsync("projectId", "chainId", "", 1, 1);
        result.ShouldNotBeNull();
        
        result = await _provider.GetProjectUserListAsync("projectId", "chainId", "address", 1, 1);
        result.ShouldNotBeNull();
        
        result = await _provider.GetProjectUserListAsync("projectId", "chainId", "elf_address", 1, 1);
        result.ShouldNotBeNull();
    }
}