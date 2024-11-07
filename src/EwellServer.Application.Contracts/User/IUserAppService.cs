using System.Threading.Tasks;
using EwellServer.User.Dtos;

namespace EwellServer.User;

public interface IUserAppService
{
    Task CreateUserAsync(UserDto user);
    Task<UserDto> GetUserByIdAsync(string userId);
}