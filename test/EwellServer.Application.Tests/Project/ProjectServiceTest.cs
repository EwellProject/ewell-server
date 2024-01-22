using EwellServer.Project.Dto;
using FluentAssertions;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace EwellServer.Project;

public class ProjectServiceTest  : EwellServerApplicationTestBase
{
    private readonly IProjectService _projectService;

    public ProjectServiceTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _projectService = GetRequiredService<IProjectService>();
    }

    [Fact]
    public async void QueryProjectAsync_Test()
    {
        var input = new QueryProjectInfoInput
        {
            ChainId = "tDVV"
        };
        var result  = _projectService.QueryProjectAsync(input);
        result.ShouldNotBeNull();
    }


}