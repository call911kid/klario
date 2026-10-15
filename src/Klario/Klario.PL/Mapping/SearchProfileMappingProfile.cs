using AutoMapper;
using Klario.PL.Contracts.SearchProfiles;
using BllResponse = Klario.BLL.Responses.SearchProfileResponse;

namespace Klario.PL.Mapping;

public class SearchProfileMappingProfile : Profile
{
    public SearchProfileMappingProfile()
    {
        CreateMap<BllResponse, SearchProfileResponse>();
    }
}
