using System.Threading.Tasks;
using EwellServer.Token.Dto;

namespace EwellServer.Token;

public interface ITokenService
{
    Task<TokenGrainDto> GetTokenAsync(string chainId, string symbol);
    
    Task<TokenPriceDto> GetTokenPriceAsync(string baseCoin, string quoteCoin);
}