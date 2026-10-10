using AutoMapper;
using Klario.BLL.Responses;
using Klario.PL.DTOs.Responses;

namespace Klario.PL.Mapping;

public class SearchProfileMappingProfile : Profile
{
    public SearchProfileMappingProfile()
    {
        CreateMap<SearchProfileResponse, SearchProfileResponseDto>();
    }
}
