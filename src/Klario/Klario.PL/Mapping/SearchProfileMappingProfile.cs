using AutoMapper;
using Klario.BLL.Queries.SearchProfiles;
using Klario.PL.Contracts.SearchProfiles;

namespace Klario.PL.Mapping;

public class SearchProfileMappingProfile : Profile
{
    public SearchProfileMappingProfile()
    {
        CreateMap<GetAllSearchProfilesScheduleResponse, ScheduleResponse>();
        CreateMap<GetAllSearchProfilesResponse, SearchProfileResponse>();

        CreateMap<GetSearchProfileByIdScheduleResponse, ScheduleResponse>();
        CreateMap<GetSearchProfileByIdResponse, SearchProfileResponse>();
    }
}
