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

public class WhitelistSyncDataServiceTest
{
    private readonly ILogger<WhitelistSyncDataService> _logger;
    private readonly IGraphQLProvider _graphQlProvider;
    private readonly IUserProjectInfoProvider _userProjectInfoGraphQlProvider;
    private readonly INESTRepository<CrowdfundingProjectIndex, string> _crowdfundingProjectIndexRepository;
    private readonly IChainAppService _chainAppService;
    private readonly WhitelistSyncDataService _service;

    public WhitelistSyncDataServiceTest()
    {
        _logger = Substitute.For<ILogger<WhitelistSyncDataService>>();
        _graphQlProvider = Substitute.For<IGraphQLProvider>();
        _userProjectInfoGraphQlProvider = Substitute.For<IUserProjectInfoProvider>();
        _crowdfundingProjectIndexRepository = Substitute.For<INESTRepository<CrowdfundingProjectIndex, string>>();
        _chainAppService = Substitute.For<IChainAppService>();
        _service = new WhitelistSyncDataService(_logger, _graphQlProvider, _userProjectInfoGraphQlProvider,
            _chainAppService, _crowdfundingProjectIndexRepository);
    }
    
    [Fact]
    public void GetBusinessType_Test()
    {
        var result = _service.GetBusinessType();
        result.ShouldBe(WorkerBusinessType.WhitelistSync);
    }
    
    [Fact]
    public async Task GetChainIdsAsync_Test()
    {
        _chainAppService.GetListAsync().Returns(new[] { "tDVV" });
        var result = await _service.GetChainIdsAsync();
        result.ShouldNotBeNull();
    }
}