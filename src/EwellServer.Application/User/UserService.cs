using System;
using System.Linq;
using System.Threading.Tasks;
using EwellServer.Common;
using EwellServer.Grains.Grain.Users;
using Microsoft.Extensions.Logging;
using Orleans;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Users;

namespace EwellServer.User;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class UserService : EwellServerAppService, IUserService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<IUserService> _logger;

    public UserService(IClusterClient clusterClient, ILogger<IUserService> logger)
    {
        _clusterClient = clusterClient;
        _logger = logger;
    }

    public async Task<string> GetCurrentUserAddressAsync(string chainId)
    {
        var userId  = CurrentUser.IsAuthenticated ? CurrentUser.GetId() : Guid.Empty;
        string userAddress = null;
        if (userId != Guid.Empty)
        {
            userAddress = await GetUserAddressAsync(chainId, userId);
        }

        _logger.LogInformation("Get current user address chainId: {chainId} address:{address}", chainId, userAddress);
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