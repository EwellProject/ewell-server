using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using EwellServer.Chains;
using EwellServer.Common;
using EwellServer.Common.GraphQL;
using EwellServer.Entities;
using EwellServer.Project.Provider;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EwellServer.Project;

public class UserProjectInfoSyncDataServiceTest
{
    private readonly ILogger<UserProjectInfoSyncDataService> _logger;
    private readonly IGraphQLProvider _graphQlProvider;
    private readonly IUserProjectInfoProvider _userProjectInfoProvider;
    private readonly INESTRepository<UserProjectInfoIndex, string> _userProjectInfoIndexRepository;
    private readonly IChainAppService _chainAppService;
    private readonly UserProjectInfoSyncDataService _service;

    public UserProjectInfoSyncDataServiceTest()
    {
        _logger = Substitute.For<ILogger<UserProjectInfoSyncDataService>>();
        _graphQlProvider = Substitute.For<IGraphQLProvider>();
        _userProjectInfoProvider = Substitute.For<IUserProjectInfoProvider>();
        _userProjectInfoIndexRepository = Substitute.For<INESTRepository<UserProjectInfoIndex, string>>();
        _chainAppService = Substitute.For<IChainAppService>();
        _service = new UserProjectInfoSyncDataService(_logger, _graphQlProvider, _userProjectInfoProvider,
            _chainAppService, _userProjectInfoIndexRepository);
    }
    
    [Fact]
    public void GetBusinessType_Test()
    {
        var result = _service.GetBusinessType();
        result.ShouldBe(WorkerBusinessType.UserProjectInfoSync);
    }
    
    [Fact]
    public async Task GetChainIdsAsync_Test()
    {
        _chainAppService.GetListAsync().Returns(new[] { "tDVV" });
        var result = await _service.GetChainIdsAsync();
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task SyncIndexerRecordsAsync_Test()
    {
        _userProjectInfoProvider.GetSyncUserProjectInfosAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>())
            .Returns(Task.FromResult(new List<UserProjectInfoIndex>()));
        var result = await _service.SyncIndexerRecordsAsync("chainId", 1, 1);
        result.ShouldBe(-1);
        
        _userProjectInfoProvider.GetSyncUserProjectInfosAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<long>())
            .Returns(Task.FromResult(new List<UserProjectInfoIndex> {new(){BlockHeight = 1}}), Task.FromResult(new List<UserProjectInfoIndex>()));
        result = await _service.SyncIndexerRecordsAsync("chainId", 1, 1);
        result.ShouldBe(1);
    }
}