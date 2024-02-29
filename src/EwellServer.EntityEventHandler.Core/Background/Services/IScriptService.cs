using System.Threading.Tasks;
using EwellServer.EntityEventHandler.Core.Background.BackgroundJobs.BackgroundJobDescriptions;

namespace EwellServer.EntityEventHandler.Core.Background.Services;

public interface IScriptService
{
    Task<int> ProcessReleaseTokenAsync(ReleaseProjectTokenJobDescription releaseProjectTokenJobDescription);
    Task ProcessCancelProjectAsync(CancelProjectJobDescription cancelProjectJobDescription);
}