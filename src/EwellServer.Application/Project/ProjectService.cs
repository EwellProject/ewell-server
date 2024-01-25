using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EwellServer.Entities;
using EwellServer.Options;
using EwellServer.Project.Dto;
using EwellServer.Project.Provider;
using EwellServer.User;
using Microsoft.Extensions.Options;
using Volo.Abp.ObjectMapping;

namespace EwellServer.Project;

public class ProjectService : EwellServerAppService, IProjectService
{
    private readonly IProjectInfoProvider _projectInfoProvider;
    private readonly IUserProjectInfoProvider _userProjectInfoProvider;
    private readonly IObjectMapper _objectMapper;
    private readonly IUserService _userService;
    private readonly IOptionsMonitor<TransactionFeeOptions> _optionsMonitor;

    public ProjectService(IProjectInfoProvider projectInfoProvider, 
        IUserProjectInfoProvider userProjectInfoProvider, IObjectMapper objectMapper, 
        IUserService userService, IOptionsMonitor<TransactionFeeOptions> optionsMonitor)
    {
        _projectInfoProvider = projectInfoProvider;
        _userProjectInfoProvider = userProjectInfoProvider;
        _objectMapper = objectMapper;
        _userService = userService;
        _optionsMonitor = optionsMonitor;
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
        var resultDto = new QueryProjectUserResultDto
        {
            TotalCount = userProjectIndex.Item1,
            Users = _objectMapper.Map<List<UserProjectInfoIndex>, List<ProjectUserDto>>(userProjectIndex.Item2),
            TotalAmount = projectIndex?.CurrentRaisedAmount ?? 0,
            TotalUser = projectIndex?.ParticipantCount ?? 0,
            VirtualAddress = projectIndex?.VirtualAddress
        };
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
}