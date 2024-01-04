using System.Threading.Tasks;

namespace EwellServer.Chains
{
    public interface IChainAppService
    {
        Task<string[]> GetListAsync();
        
        Task<string> GetChainIdAsync(int index);
    }
}