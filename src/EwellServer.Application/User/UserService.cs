using System;
using System.Linq;
using System.Threading.Tasks;
using EwellServer.Common;
using EwellServer.Grains.Grain.Users;
using Orleans;
using Volo.Abp.Users;

namespace EwellServer.User;


public interface IUserService
{
    public Task<string> GetCurrentUserAddressAsync(string chainId);
}

public class UserService : EwellServerAppService, IUserService
{
    private readonly IClusterClient _clusterClient;

    public UserService(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    public async Task<string> GetCurrentUserAddressAsync(string chainId)
    {
        var userId  = CurrentUser.IsAuthenticated ? CurrentUser.GetId() : Guid.Empty;
        string userAddress = null;
        if (userId != Guid.Empty)
        {
            userAddress = await GetUserAddressAsync(chainId, userId);
        }
        return userAddress;
    }
    
    private async Task<string> GetUserAddressAsync(string chainId, Guid userId)
    {
        if (chainId.IsNullOrEmpty() || userId == Guid.Empty)
        {
            return null;
        }

        var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
        var grainResultDto = await userGrain.GetUser();
            
        AssertHelper.IsTrue(grainResultDto.Success, "Get user fail, chainId  {chainId} userId {userId}", 
            chainId, userId);
             
        var addressInfos = grainResultDto.Data.AddressInfos;
        
        return  addressInfos?
            .Where(info => info.ChainId.Equals(chainId))
            .Select(info => info.Address)
            .FirstOrDefault();
    }
}