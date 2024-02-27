using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EwellServer.Options;
using EwellServer.Token.Index;
using EwellServer.Token.Provider;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;

namespace EwellServer.Token;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class UserTokenService : EwellServerAppService, IUserTokenService
{
    private readonly IUserTokenProvider _userTokenProvider;
    private readonly IObjectMapper _objectMapper;
    private readonly ITokenProvider _tokenProvider;
    private readonly IOptionsMonitor<UserTokenOptions> _userTokenOptionsMonitor;
    
    public UserTokenService(IUserTokenProvider userTokenProvider, IObjectMapper objectMapper, 
        ITokenProvider tokenProvider, IOptionsMonitor<UserTokenOptions> userTokenOptionsMonitor)
    {
        _userTokenProvider = userTokenProvider;
        _objectMapper = objectMapper;
        _tokenProvider = tokenProvider;
        _userTokenOptionsMonitor = userTokenOptionsMonitor;
    }

    public async Task<List<UserTokenDto>> GetUserTokensAsync(string chainId, string address)
    {
        if (chainId.IsNullOrWhiteSpace() || address.IsNullOrWhiteSpace())
        {
            return new List<UserTokenDto>();
        }

        var list = await _userTokenProvider.GetUserTokens(chainId, address);
        return list.Where(item =>
                item != null && item.Balance > 0 && _userTokenOptionsMonitor.CurrentValue.ToFilterNft(item.Symbol))
            .Select(item =>
            {
                var userTokenDto = _objectMapper.Map<IndexerUserToken, UserTokenDto>(item);
                if (userTokenDto.ImageUrl.IsNullOrWhiteSpace())
                {
                    userTokenDto.ImageUrl = _tokenProvider.BuildTokenImageUrl(userTokenDto.Symbol);
                }

                return userTokenDto;
            })
            .ToList();
    }
}