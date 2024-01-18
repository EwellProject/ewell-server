using AutoMapper;
using EwellServer.Entities;
using EwellServer.Etos;
using EwellServer.Project.Dto;
using EwellServer.Token;
using EwellServer.User.Dtos;

namespace EwellServer;

public class EwellServerApplicationAutoMapperProfile : Profile
{
    public EwellServerApplicationAutoMapperProfile()
    {
        CreateMap<UserIndex, UserDto>().ReverseMap();
        CreateMap<AddressInfo, UserAddressInfo>().ReverseMap();
        CreateMap<CrowdfundingProjectIndex, ProjectRegisteredEto>();
        CreateMap<CrowdfundingProjectIndex, ProjectCanceledEto>();
        CreateMap<CrowdfundingProjectIndex, QueryProjectResultBaseDto>();
        CreateMap<TokenGrainDto, TokenBasicInfo>()
            .ForMember(des => des.Name, opt
                => opt.MapFrom(source => source.TokenName));
        CreateMap<UserProjectInfoIndex, ProjectUserDto>()
            .ForMember(des => des.Address, opt
                => opt.MapFrom(source => source.User));
    }
}