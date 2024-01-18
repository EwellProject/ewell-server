using AutoMapper;
using EwellServer.Entities;
using EwellServer.Etos;
using EwellServer.Project.Dto;
using EwellServer.Token;
using EwellServer.Token.Index;
using EwellServer.User.Dtos;

namespace EwellServer;

public class EwellServerApplicationAutoMapperProfile : Profile
{
    public EwellServerApplicationAutoMapperProfile()
    {
        CreateMap<UserIndex, UserDto>().ReverseMap();
        CreateMap<CrowdfundingProjectIndex, ProjectRegisteredEto>();
        CreateMap<CrowdfundingProjectIndex, ProjectCanceledEto>();
        CreateMap<CrowdfundingProjectIndex, QueryProjectResultBaseDto>();
        CreateMap<IndexerUserToken, UserTokenDto>();
        CreateMap<TokenGrainDto, TokenBasicInfo>()
            .ForMember(des => des.Name, opt
                => opt.MapFrom(source => source.TokenName));
        CreateMap<UserProjectInfoIndex, ProjectUserDto>()
            .ForMember(des => des.Address, opt
                => opt.MapFrom(source => source.User));
    }
}