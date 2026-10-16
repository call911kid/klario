using AutoMapper;
using Klario.BLL.Exceptions;
using Klario.Common.Enums;
using Klario.DAL.Context;
using Klario.DAL.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Queries.SearchProfiles;

public record GetSearchProfileByIdQuery(Guid Id) : IRequest<GetSearchProfileByIdResponse>;

public sealed record GetSearchProfileByIdResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    List<string> TargetJobTitles,
    List<string> TargetLocations,
    int MaxPostingAgeMinutes,
    List<GetSearchProfileByIdScheduleResponse> Schedules,
    WorkplacePreference Workplace,
    ExperienceLevel Experience,
    JobType JobType,
    string TelegramChatId,
    string BotToken,
    DateTime CreatedAt
);

public sealed record GetSearchProfileByIdScheduleResponse(
    PostingSource Source,
    int IntervalMinutes,
    bool IsEnabled
);

public class GetSearchProfileByIdQueryHandler : IRequestHandler<GetSearchProfileByIdQuery, GetSearchProfileByIdResponse>
{
    private readonly KlarioDbContext _context;
    private readonly IMapper _mapper;

    public GetSearchProfileByIdQueryHandler(KlarioDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GetSearchProfileByIdResponse> Handle(GetSearchProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.SearchProfiles
            .AsNoTracking()
            .Include(sp => sp.Schedules)
            .FirstOrDefaultAsync(sp => sp.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new EntityNotFoundException(nameof(SearchProfile), request.Id);
        }

        return _mapper.Map<GetSearchProfileByIdResponse>(entity);
    }
}
