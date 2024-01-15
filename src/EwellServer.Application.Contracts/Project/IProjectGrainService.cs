using System.Threading.Tasks;

namespace EwellServer.Project;

public interface IProjectGrainService
{
    public Task<bool> GetProjectExistAsync(string chainId, string projectId);
    public Task SetProjectExistAsync(string chainId, string projectId, bool exist);
}