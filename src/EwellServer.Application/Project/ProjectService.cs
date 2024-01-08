using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EwellServer.Entities;
using EwellServer.Project.Dto;
using EwellServer.Project.Provider;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;

namespace EwellServer.Project;

public class ProjectService : EwellServerAppService, IProjectService
{
    private readonly IProjectInfoProvider _projectInfoProvider;
    private readonly IUserProjectInfoProvider _userProjectInfoProvider;
    private readonly IObjectMapper _objectMapper;

    public ProjectService(IProjectInfoProvider projectInfoProvider, 
        IUserProjectInfoProvider userProjectInfoProvider, IObjectMapper objectMapper)
    {
        _projectInfoProvider = projectInfoProvider;
        _userProjectInfoProvider = userProjectInfoProvider;
        _objectMapper = objectMapper;
    }

    public async Task<QueryProjectResultDto> QueryProjectAsync(QueryProjectInfoInput input)
    {
        var userId = CurrentUser.GetId();
        Dictionary<string, UserProjectInfoIndex> userProjectDict = new Dictionary<string, UserProjectInfoIndex>();

        string userAddress = null;
        if (userId != Guid.Empty)
        {
            userAddress = userId.ToString();
            userProjectDict = await _userProjectInfoProvider.GetUserProjectInfosAsync(userAddress);
        }
        var currentTime = DateTime.UtcNow;
        var tuple = await _projectInfoProvider.GetProjectInfosAsync(input, currentTime, userAddress,
            userProjectDict.Keys.ToList());

        if (tuple.Item2.IsNullOrEmpty())
        {
            return new QueryProjectResultDto();
        }

        QueryProjectResultDto resultDto = new QueryProjectResultDto();
        foreach (var info in tuple.Item2)
        {
            var resultBase = _objectMapper.Map<CrowdfundingProjectIndex, QueryProjectResultBase>(info);

            resultBase.OfResultBase(userAddress, currentTime, userProjectDict);

            resultDto.OfResultDto(userAddress, userProjectDict, resultBase);
        }

        //sorting
        resultDto.AddSorting();
        return resultDto;
    }
}