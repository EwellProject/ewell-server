using System;
using System.Threading.Tasks;
using EwellServer.Common.GraphQL;
using EwellServer.Grains.Grain.Project;
using Microsoft.Extensions.Logging;
using Orleans;

namespace EwellServer.Project;

public class ProjectGrainService : EwellServerAppService, IProjectGrainService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<GraphQLProvider> _logger;

    public ProjectGrainService(IClusterClient clusterClient, ILogger<GraphQLProvider> logger)
    {
        _clusterClient = clusterClient;
        _logger = logger;
    }

    public async Task<bool> GetProjectExistAsync(string chainId, string projectId)
    {
        try
        {
            var grain = _clusterClient.GetGrain<IProjectGrain>(projectId + "-" + chainId);
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
            var grain = _clusterClient.GetGrain<IProjectGrain>(projectId + "-" + chainId);
            await grain.SetStateAsync(exist);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "SetProjectExistAsync on chain-projectId {id}-{projectId} error ", chainId, projectId);
        }
    }
}