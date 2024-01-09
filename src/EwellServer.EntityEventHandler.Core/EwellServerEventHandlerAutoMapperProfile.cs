using AutoMapper;
using EwellServer.Entities;
using EwellServer.Users;

namespace EwellServer.EntityEventHandler.Core;

public class EwellServerEventHandlerAutoMapperProfile : Profile
{
    public EwellServerEventHandlerAutoMapperProfile()
    {
        CreateMap<UserGrainDto, UserIndex>();
    }
}