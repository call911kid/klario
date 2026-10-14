using AutoMapper;
using Klario.BLL.Commands.SearchProfiles;
using Klario.PL.Contracts.SearchProfiles;
using BllResponse = Klario.BLL.Responses.SearchProfileResponse;

namespace Klario.PL.Mapping;

public class SearchProfileMappingProfile : Profile
{
    public SearchProfileMappingProfile()
    {
        CreateMap<CreateSearchProfileRequest, CreateSearchProfileCommand>();
        CreateMap<UpdateSearchProfileRequest, UpdateSearchProfileCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<BllResponse, SearchProfileResponse>();
    }
}
