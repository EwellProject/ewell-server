using AutoMapper;
using EwellServer.Samples.Users;
using EwellServer.Samples.Users.Eto;
using EwellServer.Users.Eto;
using EwellServer.Users.Index;


namespace EwellServer.EntityEventHandler.Core;

public class EwellServerEventHandlerAutoMapperProfile : Profile
{
    public EwellServerEventHandlerAutoMapperProfile()
    {
        CreateMap<UserGrainDto, UserIndex>();
    }
}