using System.Threading.Tasks;
using EwellServer.Project.Dto;

namespace EwellServer.Project;

public interface IProjectService
{
    Task<QueryProjectResultDto> QueryProjectAsync(QueryProjectInfoInput input);
    Task<QueryProjectUserResultDto> QueryProjectUserAsync(QueryProjectUserInfoInput input);
    Task<TransactionFeeDto> GetTransactionFeeAsync();
    public Task<bool> GetProjectExistAsync(string chainId, string projectId);
    public Task SetProjectExistAsync(string chainId, string projectId, bool exist);
}