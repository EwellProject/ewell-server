using EwellServer.Grains.State.Project;
using Orleans;

namespace EwellServer.Grains.Grain.Project;

public interface IProjectGrain : IGrainWithStringKey
{
    Task SetStateAsync(bool height);
    Task<bool> GetStateAsync();
}

public class ProjectGrain : Grain<ProjectState>, IProjectGrain
{
    public override Task OnActivateAsync()
    {
        ReadStateAsync();
        return base.OnActivateAsync();
    }

    public async Task SetStateAsync(bool exist)
    {
        State.Exist = exist;
        await WriteStateAsync();
    }

    public Task<bool> GetStateAsync()
    {
        return Task.FromResult(State.Exist);
    }
}