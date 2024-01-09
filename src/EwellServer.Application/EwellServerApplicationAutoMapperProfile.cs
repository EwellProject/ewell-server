using AutoMapper;
using EwellServer.Entities;
using EwellServer.Etos;
using EwellServer.Project.Dto;
using EwellServer.Samples.Users.Dto;
using EwellServer.Samples.Users.Eto;
using EwellServer.Users;
using EwellServer.Users.Dto;

namespace EwellServer;

public class EwellServerApplicationAutoMapperProfile : Profile
{
    public EwellServerApplicationAutoMapperProfile()
    {
        CreateMap<UserSourceInput, UserGrainDto>().ReverseMap();
        CreateMap<UserGrainDto, UserDto>().ReverseMap();
        CreateMap<UserGrainDto, UserInformationEto>().ReverseMap();
        CreateMap<UserIndex, UserDto>().ReverseMap();
        CreateMap<CrowdfundingProjectIndex, ProjectRegisteredEto>();
        CreateMap<CrowdfundingProjectIndex, ProjectCanceledEto>();
        CreateMap<CrowdfundingProjectIndex, QueryProjectResultBaseDto>();
    }
}