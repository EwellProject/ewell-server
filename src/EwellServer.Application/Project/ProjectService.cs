using System.Collections.Generic;
using System.Threading.Tasks;
using EwellServer.Project.Dto;
using EwellServer.Project.Provider;

namespace EwellServer.Project;

public class ProjectService : EwellServerAppService, IProjectService
{
    private readonly IProjectInfoProvider _projectInfoProvider;
    private readonly IUserProjectInfoProvider _userProjectInfoProvider;

    public ProjectService(IProjectInfoProvider projectInfoProvider, 
        IUserProjectInfoProvider userProjectInfoProvider)
    {
        _projectInfoProvider = projectInfoProvider;
        _userProjectInfoProvider = userProjectInfoProvider;
    }

    public async Task<QueryProjectResultDto> QueryProjectAsync(QueryProjectInfoInput input)
    {

        var tuple = await _projectInfoProvider.GetProjectInfosAsync(input);

        if (tuple.Item2.IsNullOrEmpty())
        {
            return new QueryProjectResultDto();
        }

        //
        
        
        throw new System.NotImplementedException();
    }
}