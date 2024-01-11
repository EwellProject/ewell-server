using System.Threading.Tasks;

namespace EwellServer.Token;

public interface ITokenService
{
    Task<TokenGrainDto> GetTokenAsync(string chainId, string symbol);
}