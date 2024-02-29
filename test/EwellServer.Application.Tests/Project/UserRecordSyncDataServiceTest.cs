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

public class UserRecordSyncDataServiceTest
{
    private readonly ILogger<UserRecordSyncDataService> _logger;
    private readonly IGraphQLProvider _graphQlProvider;
    private readonly IUserProjectInfoProvider _userProjectInfoGraphQlProvider;
    private readonly INESTRepository<UserRecordIndex, string> _userRecordRepository;
    private readonly IChainAppService _chainAppService;
    private readonly UserRecordSyncDataService _service;

    public UserRecordSyncDataServiceTest()
    {
        _logger = Substitute.For<ILogger<UserRecordSyncDataService>>();
        _graphQlProvider = Substitute.For<IGraphQLProvider>();
        _userProjectInfoGraphQlProvider = Substitute.For<IUserProjectInfoProvider>();
        _userRecordRepository = Substitute.For<INESTRepository<UserRecordIndex, string>>();
        _chainAppService = Substitute.For<IChainAppService>();
        _service = new UserRecordSyncDataService(_logger, _graphQlProvider, _userProjectInfoGraphQlProvider,
            _chainAppService, _userRecordRepository);
    }
    
    [Fact]
    public void GetBusinessType_Test()
    {
        var result = _service.GetBusinessType();
        result.ShouldBe(WorkerBusinessType.UserRecordSync);
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
        _userProjectInfoGraphQlProvider.GetUserRecordListAsync(Arg.Any<long>(), Arg.Any<long>(),
                Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(Task.FromResult(new List<UserRecordIndex>()));
        var result = await _service.SyncIndexerRecordsAsync("chainId", 1, 1);
        result.ShouldBe(1);
        
        _userProjectInfoGraphQlProvider.GetUserRecordListAsync(Arg.Any<long>(), Arg.Any<long>(),
                Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(Task.FromResult(new List<UserRecordIndex>(){new(){BlockHeight = 1}}), 
                Task.FromResult(new List<UserRecordIndex>(){new(){BlockHeight = 2}}), 
                Task.FromResult(new List<UserRecordIndex>()));
        result = await _service.SyncIndexerRecordsAsync("chainId", 1, 1);
        result.ShouldBe(2);
    }
}