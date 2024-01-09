using EwellServer.Users;
using EwellServer.Users.Eto;
using Volo.Abp.EventBus;

namespace EwellServer.Samples.Users.Eto;

[EventName("UserInformationEto")]
public class UserInformationEto : AbstractEto<UserGrainDto>
{
    public UserInformationEto(UserGrainDto data) : base(data)
    {
    }
    
}