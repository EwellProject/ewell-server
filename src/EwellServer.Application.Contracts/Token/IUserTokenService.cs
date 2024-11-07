using System.Collections.Generic;
using System.Threading.Tasks;

namespace EwellServer.Token;

public interface IUserTokenService
{
    Task<List<UserTokenDto>> GetUserTokensAsync(string chainId, string address);
}