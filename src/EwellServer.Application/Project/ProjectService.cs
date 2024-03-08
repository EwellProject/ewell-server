using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EwellServer.Common;
using EwellServer.Common.GraphQL;
using EwellServer.Entities;
using EwellServer.Grains.Grain.Project;
using EwellServer.Options;
using EwellServer.Project.Dto;
using EwellServer.Project.Provider;
using EwellServer.User;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;

namespace EwellServer.Project;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class ProjectService : EwellServerAppService, IProjectService
{
    private readonly IProjectInfoProvider _projectInfoProvider;
    private readonly IUserProjectInfoProvider _userProjectInfoProvider;
    private readonly IObjectMapper _objectMapper;
    private readonly IUserService _userService;
    private readonly IOptionsMonitor<TransactionFeeOptions> _optionsMonitor;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<GraphQLProvider> _logger;

    public ProjectService(IProjectInfoProvider projectInfoProvider, 
        IUserProjectInfoProvider userProjectInfoProvider, IObjectMapper objectMapper, 
        IUserService userService, IOptionsMonitor<TransactionFeeOptions> optionsMonitor, 
        ILogger<GraphQLProvider> logger, IClusterClient clusterClient)
    {
        _projectInfoProvider = projectInfoProvider;
        _userProjectInfoProvider = userProjectInfoProvider;
        _objectMapper = objectMapper;
        _userService = userService;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
        _clusterClient = clusterClient;
    }

    public async Task<QueryProjectResultDto> QueryProjectAsync(QueryProjectInfoInput input)
    {
        var userAddress = await _userService.GetCurrentUserAddressAsync(input.ChainId);

        if (input.QuerySelf() && userAddress.IsNullOrEmpty())
        {
            return new QueryProjectResultDto();
        }

        var userProjectDict = await _userProjectInfoProvider.GetUserProjectInfosAsync(userAddress);

        var currentTime = DateTime.UtcNow;
        var tuple = await _projectInfoProvider.GetProjectInfosAsync(input, currentTime, userAddress,
            userProjectDict.Keys.ToList());

        if (tuple.Item2.IsNullOrEmpty())
        {
            return new QueryProjectResultDto();
        }

        var resultDto = new QueryProjectResultDto
        {
            TotalCount = tuple.Item1
        };
        foreach (var info in tuple.Item2)
        {
            var resultBase = _objectMapper.Map<CrowdfundingProjectIndex, QueryProjectResultBaseDto>(info);

            resultBase.OfResultBase(userAddress, currentTime, userProjectDict);

            resultDto.OfResultDto(userAddress, input.ProjectId, input.Types, resultBase, userProjectDict);
        }

        //sorting
        resultDto.AddSorting();
        return resultDto;
    }
    
    public async Task<QueryProjectUserResultDto> QueryProjectUserAsync(QueryProjectUserInfoInput input)
    {
        var userProjectIndex = await _userProjectInfoProvider.GetProjectUserListAsync(input.ProjectId, input.ChainId, 
            input.Address, input.MaxResultCount, input.SkipCount);
        var projectIndex = await _projectInfoProvider.GetProjectInfosAsync(input.ChainId, input.ProjectId);
        if (projectIndex == null)
        {
            return new QueryProjectUserResultDto();
        }
        var resultDto = _objectMapper.Map<CrowdfundingProjectIndex, QueryProjectUserResultDto>(projectIndex);
        resultDto.Users = _objectMapper.Map<List<UserProjectInfoIndex>, List<ProjectUserDto>>(userProjectIndex.Item2);
        resultDto.TotalCount = userProjectIndex.Item1;
        resultDto.TotalAmount = projectIndex.CurrentRaisedAmount;
        resultDto.TotalUser = projectIndex.ParticipantCount;
        return resultDto;
    }

    public Task<TransactionFeeDto> GetTransactionFeeAsync()
    {
        var transactionFeeOptions = _optionsMonitor.CurrentValue;
        return Task.FromResult(new TransactionFeeDto
        {
            TransactionFee = transactionFeeOptions.TransactionFee
        });    
    }
    
    public async Task<bool> GetProjectExistAsync(string chainId, string projectId)
    {
        try
        {
            var grain = _clusterClient.GetGrain<IProjectGrain>(GuidHelper.GenerateId(projectId, chainId));
            return await grain.GetStateAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetProjectExistAsync on chain-projectId {id}-{projectId} error", chainId, projectId);
            return false;
        }
    }

    public async Task SetProjectExistAsync(string chainId, string projectId, bool exist)
    {
        try
        {
            var grain = _clusterClient.GetGrain<IProjectGrain>(GuidHelper.GenerateId(projectId, chainId));
            await grain.SetStateAsync(exist);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "SetProjectExistAsync on chain-projectId {id}-{projectId} error ", chainId, projectId);
        }
    }
}