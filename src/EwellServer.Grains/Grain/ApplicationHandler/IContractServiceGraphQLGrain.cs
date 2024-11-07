using Orleans;

namespace EwellServer.Grains.Grain.ApplicationHandler;

public interface IContractServiceGraphQLGrain : IGrainWithStringKey
{
    Task SetStateAsync(long height);
    Task<long> GetStateAsync();
}