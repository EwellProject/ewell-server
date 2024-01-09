using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EwellServer.Entities;
using EwellServer.Grains.Grain.Users;
using EwellServer.Project.Dto;
using EwellServer.Project.Provider;
using Orleans;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;

namespace EwellServer.Project;

public class ProjectService : EwellServerAppService, IProjectService
{
    private readonly IProjectInfoProvider _projectInfoProvider;
    private readonly IUserProjectInfoProvider _userProjectInfoProvider;
    private readonly IObjectMapper _objectMapper;
    private readonly IClusterClient _clusterClient;
    public ProjectService(IProjectInfoProvider projectInfoProvider, 
        IUserProjectInfoProvider userProjectInfoProvider, IObjectMapper objectMapper, IClusterClient clusterClient)
    {
        _projectInfoProvider = projectInfoProvider;
        _userProjectInfoProvider = userProjectInfoProvider;
        _objectMapper = objectMapper;
        _clusterClient = clusterClient;
    }

    public async Task<QueryProjectResultDto> QueryProjectAsync(QueryProjectInfoInput input)
    {
        var userId = CurrentUser.GetId();
        Dictionary<string, UserProjectInfoIndex> userProjectDict = new Dictionary<string, UserProjectInfoIndex>();

        string userAddress = null;
        if (userId != Guid.Empty)
        {
            var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
            var user = await userGrain.GetUserAsync();
            userAddress = user.Data?.AelfAddress;
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

            resultDto.OfResultDto(userAddress, resultBase, userProjectDict);
        }

        //sorting
        resultDto.AddSorting();
        return resultDto;
    }
}