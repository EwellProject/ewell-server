using AutoMapper;
using EwellServer.Entities;
using EwellServer.Samples.Users;
using EwellServer.Samples.Users.Eto;
using EwellServer.Users.Eto;

namespace EwellServer.EntityEventHandler.Core;

public class EwellServerEventHandlerAutoMapperProfile : Profile
{
    public EwellServerEventHandlerAutoMapperProfile()
    {
        CreateMap<UserGrainDto, UserIndex>();
    }
}