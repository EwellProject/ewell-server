using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Elasticsearch.Net;
using EwellServer.Common.GraphQL;
using EwellServer.Entities;
using EwellServer.Project.Dto;
using Microsoft.Extensions.Logging;
using Nest;
using NSubstitute;
using Xunit;

namespace EwellServer.Project.Provider;

public class ProjectInfoProviderTest 
{
    private readonly ILogger<IProjectInfoProvider> _mockLogger;
    private readonly IProjectInfoProvider _provider;
    private readonly INESTRepository<CrowdfundingProjectIndex, string> _mockCrowdfundingProjectIndexRepository;
    private readonly IGraphQlHelper _mockGraphQlHelper;

    public ProjectInfoProviderTest()
    {
        _mockLogger = Substitute.For<ILogger<IProjectInfoProvider>>();
        _mockCrowdfundingProjectIndexRepository = Substitute.For<INESTRepository<CrowdfundingProjectIndex, string>>();
        _mockGraphQlHelper = Substitute.For<IGraphQlHelper>();
        _provider = new ProjectInfoProvider(_mockCrowdfundingProjectIndexRepository);
    }
    
    [Fact]
    public void PrintExeScript_Test()
    {
        QueryProjectInfoInput input = new QueryProjectInfoInput()
        {
            ChainId = "tDVV",
            Types = new List<ProjectType>()
            {
                //ProjectType.Active,
                ProjectType.Closed,
                //ProjectType.Created,
                //ProjectType.Participate
            }
        };

        var currentTime = DateTime.Now;
        var userAddress = "123";
        var userProjectIds = new List<string> { "projectId1", "projectId2" };

        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("ewellserver.crowdfundingprojectindex");

        var client = new ElasticClient(settings);


        var mustQuery = new List<Func<QueryContainerDescriptor<CrowdfundingProjectIndex>, QueryContainer>>();

        var shouldQuery = new List<Func<QueryContainerDescriptor<CrowdfundingProjectIndex>, QueryContainer>>();

        if (!input.ChainId.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(i =>
                i.Field(f => f.ChainId).Value(input.ChainId)));
        }

        if (!input.ProjectId.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(i =>
                i.Field(f => f.Id).Value(input.ProjectId)));
        }

        //ProjectInfoProvider.AssemblyStatusQuery(input.Status, mustQuery, currentTime);
       // ProjectInfoProvider.AssemblyProjectTypesQuery(input.Types, shouldQuery, currentTime, userAddress, userProjectIds);

        if (shouldQuery.Any())
        {
            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }

        QueryContainer Filter(QueryContainerDescriptor<CrowdfundingProjectIndex> descriptor)
        {
            return descriptor.Bool(b => b.Must(mustQuery));
        }
        
        var searchRequest = new SearchDescriptor<CrowdfundingProjectIndex>()
            .Query(q => Filter(q))
            .From(input.SkipCount)
            .Size(input.MaxResultCount)
            .Sort(s => s.Descending(p => p.CreateTime));

        var jsonString = client.RequestResponseSerializer.SerializeToString(searchRequest);
        
        _mockLogger.LogInformation(jsonString);
    }

    [Fact]
    public async Task GetProjectInfosAsync_Test()
    {
        // Arrange
        var expectedResult = MockProjectInfos();
        
        _mockCrowdfundingProjectIndexRepository.GetSortListAsync(
                Arg.Any<Func<QueryContainerDescriptor<CrowdfundingProjectIndex>, QueryContainer>>(),
                null, // Or your specific delegate if needed
                Arg.Any<Func<SortDescriptor<CrowdfundingProjectIndex>, IPromise<IList<ISort>>>>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>())
            .Returns(Task.FromResult(expectedResult));
        
        
        var input = new QueryProjectInfoInput
        {
            ChainId = "tDVV"
        };
        var currentTime = DateTime.UtcNow;
        var userAddress = "vQfjcuW3RbGmkcL74YY4q3BX9UcH5rmwLmbQi3PsZxg8vE9Uk";
        var userProjectIds = new List<string> { "1", "2" };
        var result = await _provider.GetProjectInfosAsync(input, currentTime, userAddress, userProjectIds);
        var count = result.Item1;
        Assert.NotNull(result);
        Assert.Equal(expectedResult.Item1, result.Item1);
    }
    
    [Fact]
    public async Task GetProjectInfosAsync_Test2()
    {
        var expectedResult = new CrowdfundingProjectIndex
        {
            Id = "1",
            CurrentRaisedAmount = 100,
            ChainId = "tDVV",
            Creator = "vQfjcuW3RbGmkcL74YY4q3BX9UcH5rmwLmbQi3PsZxg8vE9Uk"
        };
        
        _mockCrowdfundingProjectIndexRepository.GetAsync(Arg.Any<Func<QueryContainerDescriptor<CrowdfundingProjectIndex>, QueryContainer>>())
            .Returns(Task.FromResult(expectedResult));
        var result = await _provider.GetProjectInfosAsync("chainId", "projectId");
        Assert.NotNull(result);
    }

    private Tuple<long, List<CrowdfundingProjectIndex>> MockProjectInfos()
    {
        
        var expectedProjects = new List<CrowdfundingProjectIndex>
        {
            new ()
            {
                Id = "1",
                CurrentRaisedAmount = 100,
                ChainId = "tDVV",
                Creator = "vQfjcuW3RbGmkcL74YY4q3BX9UcH5rmwLmbQi3PsZxg8vE9Uk"
            },
            new ()
            {
                Id = "2",
                CurrentRaisedAmount = 200,
                ChainId = "tDVV",
                Creator = "xzNtLTgrkARsfvkthHFJMbXTcRbwQgz2UyZ8NKjiE4p7tFdMP"
            }
        };
        return new Tuple<long, List<CrowdfundingProjectIndex>>(expectedProjects.Count, expectedProjects);
    }
}