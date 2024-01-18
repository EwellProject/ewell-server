using System;
using System.Collections.Generic;
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
        if(chainId.IsNullOrWhiteSpace() || address.IsNullOrWhiteSpace())
        {
            return new List<UserTokenDto>();
        }
        var list = await _userTokenProvider.GetUserTokens(chainId, address);
        return _objectMapper.Map<List<IndexerUserToken>, List<UserTokenDto>>(list);
    }
}