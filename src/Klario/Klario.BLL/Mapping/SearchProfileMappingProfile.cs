using AutoMapper;
using Klario.BLL.Commands.SearchProfiles;
using Klario.BLL.Queries.SearchProfiles;
using Klario.DAL.Models;

namespace Klario.BLL.Mapping;

public class SearchProfileMappingProfile : Profile
{
    public SearchProfileMappingProfile()
    {
        CreateMap<CreateSearchProfileCommand, SearchProfile>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Schedules, opt => opt.Ignore());

        CreateMap<UpdateSearchProfileCommand, SearchProfile>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Schedules, opt => opt.Ignore());

        CreateMap<SearchProfile, GetAllSearchProfilesResponse>();
        CreateMap<Schedule, GetAllSearchProfilesScheduleResponse>();
        
        CreateMap<SearchProfile, GetSearchProfileByIdResponse>();
        CreateMap<Schedule, GetSearchProfileByIdScheduleResponse>();
    }
}
