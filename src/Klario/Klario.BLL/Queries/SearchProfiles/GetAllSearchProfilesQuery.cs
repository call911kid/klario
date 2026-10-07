using AutoMapper;
using Klario.BLL.Responses;
using Klario.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Queries.SearchProfiles;

public record GetAllSearchProfilesQuery : IRequest<List<SearchProfileResponse>>;

public class GetAllSearchProfilesQueryHandler : IRequestHandler<GetAllSearchProfilesQuery, List<SearchProfileResponse>>
{
    private readonly KlarioDbContext _context;
    private readonly IMapper _mapper;

    public GetAllSearchProfilesQueryHandler(KlarioDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<SearchProfileResponse>> Handle(GetAllSearchProfilesQuery request, CancellationToken cancellationToken)
    {
        var entities = await _context.SearchProfiles
            .AsNoTracking()
            .OrderByDescending(sp => sp.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<SearchProfileResponse>>(entities);
    }
}
