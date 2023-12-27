using AutoMapper;
using EwellServer.Samples.Users;
using EwellServer.Samples.Users.Dto;
using EwellServer.Samples.Users.Eto;
using EwellServer.Users.Index;

namespace EwellServer;

public class EwellServerApplicationAutoMapperProfile : Profile
{
    public EwellServerApplicationAutoMapperProfile()
    {
        CreateMap<UserSourceInput, UserGrainDto>().ReverseMap();
        CreateMap<UserGrainDto, UserDto>().ReverseMap();
        CreateMap<UserGrainDto, UserInformationEto>().ReverseMap();
        CreateMap<UserIndex, UserDto>().ReverseMap();
    }
}