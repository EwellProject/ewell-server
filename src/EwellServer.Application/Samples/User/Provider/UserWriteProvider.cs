using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using EwellServer.Common;
using EwellServer.Grains.Grain.Users;
using EwellServer.Samples.Users;
using EwellServer.Samples.Users.Dto;
using EwellServer.Samples.Users.Eto;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace EwellServer.Samples.User.Provider;

public interface IUserWriteProvider
{
    public Task<UserDto> SaveUserAsync(UserGrainDto userSourceInput);

}

public class UserWriteProvider: IUserWriteProvider, ISingletonDependency
{
    private readonly ILogger<UserWriteProvider> _logger;
    private readonly IClusterClient _clusterClient;
    private readonly IObjectMapper _objectMapper;
    private readonly IDistributedEventBus _distributedEventBus;
    
    
    public UserWriteProvider(IClusterClient clusterClient, IObjectMapper objectMapper, 
                                   ILogger<UserWriteProvider> logger,IDistributedEventBus distributedEventBus)
    {
        _clusterClient = clusterClient;
        _objectMapper = objectMapper;
        _logger = logger;
        _distributedEventBus = distributedEventBus;
    }

    public async Task<UserDto> SaveUserAsync(UserGrainDto userGrainDto)
    {
        try
        {
            var userGrain = _clusterClient.GetGrain<IUserGrain>(userGrainDto.Id);
            var result = await userGrain.UpdateUserAsync(userGrainDto);
            AssertHelper.IsTrue(result.Success, "Save user fail.");
            
            // publish event to wright data to ES
            await _distributedEventBus.PublishAsync(new UserInformationEto(result.Data));
            return _objectMapper.Map<UserGrainDto, UserDto>(result.Data);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "save error!");
            throw;
        }
    }

}