using AElf.Client.Service;

namespace EwellServer.EntityEventHandler.Core.Background.Providers
{
    public interface IAElfClientProvider
    {
        AElfClient GetClient(string chainName);
    }
}