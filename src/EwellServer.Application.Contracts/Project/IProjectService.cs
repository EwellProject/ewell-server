using System.Threading.Tasks;
using EwellServer.Project.Dto;

namespace EwellServer.Project;

public interface IProjectService
{
    Task<QueryProjectResultDto> QueryProjectAsync(QueryProjectInfoInput input);
}