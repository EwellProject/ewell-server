using System.Collections.Generic;
using System.Threading.Tasks;
using EwellServer.Token;
using EwellServer.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace EwellServer.Controllers.Project;


[RemoteService]
[Area("app")]
[ControllerName("Token")]
[Route("api/app/token")]
public class TokenController
{
    private readonly IUserTokenService _userTokenService;
    private readonly IUserService _userService;

    public TokenController(IUserTokenService userTokenService, IUserService userService)
    {
        _userTokenService = userTokenService;
        _userService = userService;
    }

    [HttpGet]
    [Route("list")]
    [Authorize]
    public async Task<List<UserTokenDto>> GetTokenAsync(string chainId)
    {
        var userAddress = await _userService.GetCurrentUserAddressAsync(chainId);
        return await _userTokenService.GetUserTokensAsync(chainId, userAddress);
    }
}