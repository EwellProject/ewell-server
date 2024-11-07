using System.Threading.Tasks;

namespace EwellServer.User;

public interface IUserService
{
    public Task<string> GetCurrentUserAddressAsync(string chainId);
}