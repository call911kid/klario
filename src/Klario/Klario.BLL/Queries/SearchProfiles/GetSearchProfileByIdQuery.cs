using AutoMapper;
using Klario.BLL.Exceptions;
using Klario.BLL.Responses;
using Klario.DAL.Context;
using Klario.DAL.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Queries.SearchProfiles;

public record GetSearchProfileByIdQuery(Guid Id) : IRequest<SearchProfileResponse>;

public class GetSearchProfileByIdQueryHandler : IRequestHandler<GetSearchProfileByIdQuery, SearchProfileResponse>
{
    private readonly KlarioDbContext _context;
    private readonly IMapper _mapper;

    public GetSearchProfileByIdQueryHandler(KlarioDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SearchProfileResponse> Handle(GetSearchProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.SearchProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new EntityNotFoundException(nameof(SearchProfile), request.Id);
        }

        return _mapper.Map<SearchProfileResponse>(entity);
    }
}
