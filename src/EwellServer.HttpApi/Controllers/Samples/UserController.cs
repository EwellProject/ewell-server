using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EwellServer.Common.Dtos;
using EwellServer.Samples.Users;
using EwellServer.Samples.Users.Dto;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace EwellServer.Controllers.Samples;

[RemoteService]
[Area("app")]
[ControllerName("SampleDemo")]
[Route("api/app/users")]
public class UserDemoController : AbpController
{
    private readonly IUserAppService _userAppService;

    public UserDemoController(IUserAppService userAppService)
    {
        _userAppService = userAppService;
    }

    /// post method used to update data
    [HttpPost]
    public async Task<UserDto> AddUser(UserSourceInput userSourceInput)
    {
        return await _userAppService.AddUserAsync(userSourceInput);
    }

    /// post method used to query data
    [HttpGet]
    public async Task<UserDto> GetUserById(string userId)
    {
        return await _userAppService.GetByIdAsync(userId);
    }

    [HttpGet("page")]
    public async Task<PageResultDto<UserDto>> QueryUserPager(string address)
    {
        return await _userAppService.QueryUserPagerAsync(new UserQueryRequestDto(0, 10)
        {
            Address = address
        });
    }
}