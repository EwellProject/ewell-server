using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EwellServer.Common.GraphQL;
using EwellServer.Entities;
using EwellServer.Grains.Grain.Project;
using EwellServer.Options;
using EwellServer.Project.Dto;
using EwellServer.Project.Provider;
using EwellServer.User;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Orleans;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace EwellServer.Project;

public class ProjectServiceTest
{
    private readonly IProjectService _projectService;
    private readonly IProjectInfoProvider _mockProjectInfoProvider;
    private readonly IUserProjectInfoProvider _mockUserProjectInfoProvider;
    private readonly IObjectMapper _mockObjectMapper;
    private readonly IUserService _mockUserService;
    private readonly IOptionsMonitor<TransactionFeeOptions> _mockOptionsMonitor;
    private readonly IClusterClient _mockClusterClient;
    private readonly ILogger<GraphQLProvider> _mockLogger;

    public ProjectServiceTest()
    {
        _mockProjectInfoProvider = Substitute.For<IProjectInfoProvider>();
        _mockUserProjectInfoProvider = Substitute.For<IUserProjectInfoProvider>();
        _mockObjectMapper = Substitute.For<IObjectMapper>();
        _mockUserService = Substitute.For<IUserService>();
        _mockOptionsMonitor = Substitute.For<IOptionsMonitor<TransactionFeeOptions>>();
        _mockClusterClient = Substitute.For<IClusterClient>();
        _mockLogger = Substitute.For<ILogger<GraphQLProvider>>();
        _projectService = new ProjectService(_mockProjectInfoProvider, _mockUserProjectInfoProvider, _mockObjectMapper, 
            _mockUserService, _mockOptionsMonitor, _mockLogger, _mockClusterClient);
    }

    [Fact]
    public async void QueryProjectAsync_Test()
    {
        var input = new QueryProjectInfoInput
        {
            ChainId = "tDVV",
            Types = new List<ProjectType>
            {
                ProjectType.Created
            }
        };
        // first return
        _mockUserService.GetCurrentUserAddressAsync(Arg.Any<string>()).Returns(Task.FromResult(""));
        var result  = await _projectService.QueryProjectAsync(input);
        result.ShouldNotBeNull();
        
        // second return
        _mockUserService.GetCurrentUserAddressAsync(Arg.Any<string>()).Returns(Task.FromResult("userAddress"));
        _mockUserProjectInfoProvider.GetUserProjectInfosAsync(Arg.Any<string>())
            .Returns(new Dictionary<string, UserProjectInfoIndex> { ["id"] = new UserProjectInfoIndex() });
        _mockProjectInfoProvider.GetProjectInfosAsync(Arg.Any<QueryProjectInfoInput>(), Arg.Any<DateTime>(),
                Arg.Any<string>(), Arg.Any<List<string>>())
            .Returns(Task.FromResult(
                new Tuple<long, List<CrowdfundingProjectIndex>>(1, new List<CrowdfundingProjectIndex>())));
        result  = await _projectService.QueryProjectAsync(input);
        result.ShouldNotBeNull();
        
        // last return
        _mockProjectInfoProvider.GetProjectInfosAsync(Arg.Any<QueryProjectInfoInput>(), Arg.Any<DateTime>(),
                Arg.Any<string>(), Arg.Any<List<string>>())
            .Returns(Task.FromResult(
                new Tuple<long, List<CrowdfundingProjectIndex>>(1, new List<CrowdfundingProjectIndex>() { new() })));
        _mockObjectMapper.Map<CrowdfundingProjectIndex, QueryProjectResultBaseDto>(Arg.Any<CrowdfundingProjectIndex>())
            .Returns(new QueryProjectResultBaseDto(){Id = "id", Creator = "creator"});
        result  = await _projectService.QueryProjectAsync(input);
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async void QueryProjectUserAsync_Test()
    {
        var input = new QueryProjectUserInfoInput
        {
            ChainId = "tDVV",
            MaxResultCount = 1,
            SkipCount = 0,
            Address = "address"
        };
        _mockObjectMapper.Map<CrowdfundingProjectIndex, QueryProjectUserResultDto>(Arg.Any<CrowdfundingProjectIndex>())
            .Returns(new QueryProjectUserResultDto(){TotalCount = 1});
        _mockUserProjectInfoProvider.GetProjectUserListAsync(Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(Task.FromResult(new Tuple<long, List<UserProjectInfoIndex>>(1, new List<UserProjectInfoIndex> { new() })));
        _mockProjectInfoProvider.GetProjectInfosAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(new CrowdfundingProjectIndex()));
        var result  = await _projectService.QueryProjectUserAsync(input);
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async void GetTransactionFeeAsync_Test()
    {
        _mockOptionsMonitor.CurrentValue.Returns(new TransactionFeeOptions { TransactionFee = new decimal(0.1) });
        var result  = await _projectService.GetTransactionFeeAsync();
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async void GetProjectExistAsync_Test()
    {
        _mockClusterClient.GetGrain<IProjectGrain>(Arg.Any<string>()).Returns(new ProjectGrain());
        var result  = await _projectService.GetProjectExistAsync("chainId", "projectId");
        result.ShouldBe(false);
    }
    
    [Fact]
    public async void SetProjectExistAsync_Test()
    {
        await _projectService.SetProjectExistAsync("chainId", "projectId", true);
    }
}