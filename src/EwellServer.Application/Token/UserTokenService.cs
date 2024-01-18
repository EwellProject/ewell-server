using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EwellServer.Token.Index;
using EwellServer.Token.Provider;
using Volo.Abp.ObjectMapping;

namespace EwellServer.Token;

public class UserTokenService : EwellServerAppService, IUserTokenService
{
    private readonly IUserTokenProvider _userTokenProvider;
    private readonly IObjectMapper _objectMapper;
    
    public UserTokenService(IUserTokenProvider userTokenProvider, IObjectMapper objectMapper)
    {
        _userTokenProvider = userTokenProvider;
        _objectMapper = objectMapper;
    }

    public async Task<List<UserTokenDto>> GetUserTokensAsync(string chainId, string address)
    {
        if (chainId.IsNullOrWhiteSpace() || address.IsNullOrWhiteSpace())
        {
            return new List<UserTokenDto>();
        }

        var list = await _userTokenProvider.GetUserTokens(chainId, address);
        return list.Where(item => item != null)
            .Select(item => _objectMapper.Map<IndexerUserToken, UserTokenDto>(item))
            .ToList();
    }
}