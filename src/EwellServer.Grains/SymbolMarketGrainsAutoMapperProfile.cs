using AutoMapper;
using EwellServer.Grains.Grain.Users;
using EwellServer.Grains.State.Users;
using EwellServer.Samples.Users;
using EwellServer.Samples.Users.Dto;
using EwellServer.Samples.Users.Eto;
using EwellServer.Users.Eto;

namespace EwellServer.Grains;

public class SymbolMarketGrainsAutoMapperProfile : Profile
{
    public SymbolMarketGrainsAutoMapperProfile()
    {
        CreateMap<UserGrainDto, UserState>().ReverseMap();
        CreateMap<UserGrainDto, UserDto>().ReverseMap();
        CreateMap<UserGrainDto, UserInformationEto>().ReverseMap();
        
    }
}