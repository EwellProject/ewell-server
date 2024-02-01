using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using EwellServer.Chains;
using EwellServer.Common;
using EwellServer.Common.GraphQL;
using EwellServer.Entities;
using EwellServer.Project.Provider;
using EwellServer.Token;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace EwellServer.Project;

public class ProjectInfoSyncDataServiceTest
{
    private readonly ILogger<ProjectInfoSyncDataService> _logger;
    private readonly IGraphQLProvider _graphQlProvider;
    private readonly IUserProjectInfoProvider _userProjectInfoGraphQlProvider;
    private readonly INESTRepository<CrowdfundingProjectIndex, string> _crowdfundingProjectIndexRepository;
    private readonly IChainAppService _chainAppService;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly IObjectMapper _objectMapper;
    private readonly ITokenService _tokenService;
    private readonly IProjectService _projectService;
    private readonly ProjectInfoSyncDataService _service;

    public ProjectInfoSyncDataServiceTest()
    {
        _logger = Substitute.For<ILogger<ProjectInfoSyncDataService>>();
        _graphQlProvider = Substitute.For<IGraphQLProvider>();
        _userProjectInfoGraphQlProvider = Substitute.For<IUserProjectInfoProvider>();
        _crowdfundingProjectIndexRepository = Substitute.For<INESTRepository<CrowdfundingProjectIndex, string>>();
        _chainAppService = Substitute.For<IChainAppService>();
        _distributedEventBus = Substitute.For<IDistributedEventBus>();
        _objectMapper = Substitute.For<IObjectMapper>();
        _tokenService = Substitute.For<ITokenService>();
        _projectService = Substitute.For<IProjectService>();
        _service = new ProjectInfoSyncDataService(_logger, _graphQlProvider, _userProjectInfoGraphQlProvider, _chainAppService, _crowdfundingProjectIndexRepository,
            _distributedEventBus, _objectMapper, _tokenService, _projectService);
    }

    [Fact]
    public void GetBusinessType_Test()
    {
        var result = _service.GetBusinessType();
        result.ShouldBe(WorkerBusinessType.ProjectInfoSync);
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
        // first return
        _userProjectInfoGraphQlProvider.GetProjectListAsync(Arg.Any<long>(), Arg.Any<long>(),
                Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(Task.FromResult(new List<CrowdfundingProjectIndex>()));   
        var result = await _service.SyncIndexerRecordsAsync("chainId", 1, 1);
        result.ShouldBe(1);
        
        // second return
        _userProjectInfoGraphQlProvider.GetProjectListAsync(Arg.Any<long>(), Arg.Any<long>(),
                Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(
                Task.FromResult(new List<CrowdfundingProjectIndex> { new()
                {
                    BlockHeight = 1,
                    TokenReleaseTime = new DateTime(),
                    TotalPeriod = 1,
                    ToRaiseToken = new TokenBasicInfo { ChainId = "tDVV", Symbol = "elf" },
                    CrowdFundingIssueToken = new TokenBasicInfo { ChainId = "tDVV", Symbol = "elf" },
                    IsCanceled = false } }),
                Task.FromResult(new List<CrowdfundingProjectIndex> { new()
                {
                    BlockHeight = 2,
                    TokenReleaseTime = new DateTime(),
                    TotalPeriod = 1,
                    ToRaiseToken = new TokenBasicInfo { ChainId = "tDVV", Symbol = "elf" },
                    CrowdFundingIssueToken = new TokenBasicInfo { ChainId = "tDVV", Symbol = "elf" },
                    IsCanceled = true } }),
                Task.FromResult(new List<CrowdfundingProjectIndex>())
                );
        _tokenService.GetTokenAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(new TokenGrainDto()));
        _projectService.GetProjectExistAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(true), Task.FromResult(false));
        result = await _service.SyncIndexerRecordsAsync("chainId", 1, 1);
        result.ShouldBe(2);
    }
}