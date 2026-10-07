using AutoMapper;
using Klario.BLL.Commands.SearchProfiles;
using Klario.PL.DTOs.Requests;
using Klario.PL.DTOs.Responses;

namespace Klario.PL.Mapping;

public class SearchProfileDtoMappingProfile : Profile
{
    public SearchProfileDtoMappingProfile()
    {
        CreateMap<CreateSearchProfileRequestDto, CreateSearchProfileCommand>();
        CreateMap<UpdateSearchProfileRequestDto, UpdateSearchProfileCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

    }
}
