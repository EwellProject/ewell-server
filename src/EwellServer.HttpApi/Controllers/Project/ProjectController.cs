using System;
using System.Threading.Tasks;
using EwellServer.EntityEventHandler.Core.Handler;
using EwellServer.Etos;
using EwellServer.Project;
using EwellServer.Project.Dto;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.EventBus.Distributed;

namespace EwellServer.Controllers.Project;

[RemoteService]
[Area("app")]
[ControllerName("Project")]
[Route("api/app/project")]
public class ProjectController
{
    private readonly IProjectService _projectService;
    private readonly IDistributedEventBus _distributedEventBus;

    public ProjectController(IProjectService projectService, IDistributedEventBus distributedEventBus)
    {
        _projectService = projectService;
        _distributedEventBus = distributedEventBus;
    }
    
    [HttpGet]
    [Route("list")]
    public async Task<QueryProjectResultDto> QueryProjectAsync(QueryProjectInfoInput input)
    {
        return await _projectService.QueryProjectAsync(input);
    }
    
    [HttpGet]
    [Route("userList")]
    public async Task<QueryProjectUserResultDto> QueryUserListAsync(QueryProjectUserInfoInput input)
    {
        return await _projectService.QueryProjectUserAsync(input);
    }
    
    [HttpGet]
    [Route("fee")]
    public async Task<TransactionFeeDto> GetTransactionFeeAsync()
    {
        return await _projectService.GetTransactionFeeAsync();
    }
    
    [HttpGet]
    [Route("test")]
    public async Task Test()
    {
        await _distributedEventBus.PublishAsync(new ProjectRegisteredEto()
        {
            ChainId = "tDVV",
            Id = "039b11da4acd3b35f001b014fc2a6cf24172bf172ef36b61852570756cfbce14",
            TotalPeriod = 1,
            PeriodDuration = 0,
            EndTime = new DateTime()
        });
    }
}