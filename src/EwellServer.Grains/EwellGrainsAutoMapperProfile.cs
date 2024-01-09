using AutoMapper;
using EwellServer.Grains.Grain.Users;
using EwellServer.Grains.State.Token;
using EwellServer.Grains.State.Users;
using EwellServer.Samples.Users;
using EwellServer.Samples.Users.Dto;
using EwellServer.Samples.Users.Eto;
using EwellServer.Token;
using EwellServer.Users.Eto;

namespace EwellServer.Grains;

public class EwellGrainsAutoMapperProfile : Profile
{
    public EwellGrainsAutoMapperProfile()
    {
        CreateMap<UserGrainDto, UserState>().ReverseMap();
        CreateMap<UserGrainDto, UserDto>().ReverseMap();
        CreateMap<UserGrainDto, UserInformationEto>().ReverseMap();
        CreateMap<TokenGrainDto, TokenState>().ReverseMap();
    }
}