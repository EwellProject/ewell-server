using System.Threading.Tasks;
using EwellServer.Project;
using EwellServer.Project.Dto;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace EwellServer.Controllers.Project;

[RemoteService]
[Area("app")]
[ControllerName("Project")]
[Route("api/app/project")]
public class ProjectController
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }
    
    [HttpGet]
    [Route("list")]
    public async Task<QueryProjectResultDto> QueryProjectAsync(QueryProjectInfoInput input)
    {
        return await _projectService.QueryProjectAsync(input);
    }
}