using AutoMapper;
using EwellServer.Grains.Grain.Users;
using EwellServer.Grains.State.Token;
using EwellServer.Grains.State.Users;
using EwellServer.Token;
using EwellServer.User.Dtos;

namespace EwellServer.Grains;

public class EwellGrainsAutoMapperProfile : Profile
{
    public EwellGrainsAutoMapperProfile()
    {
        CreateMap<UserGrainDto, UserState>().ReverseMap();
        CreateMap<UserState, UserDto>().ReverseMap();
        CreateMap<TokenGrainDto, TokenState>().ReverseMap();
    }
}