using AutoMapper;
using Klario.DAL.Context;
using Klario.Common.Enums;
using Klario.DAL.Models;
using MediatR;

namespace Klario.BLL.Commands.SearchProfiles;

public record CreateSearchProfileCommand(
    string Name,
    string? Description,
    List<string> TargetJobTitles,
    List<string> TargetLocations,
    TimeSpan MaxPostingAge,
    WorkplacePreference Workplace,
    ExperienceLevel Experience,
    JobType JobType,
    string TelegramChatId,
    string BotToken
) : IRequest<Guid>;

public class CreateSearchProfileCommandHandler : IRequestHandler<CreateSearchProfileCommand, Guid>
{
    private readonly KlarioDbContext _context;
    private readonly IMapper _mapper;

    public CreateSearchProfileCommandHandler(KlarioDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Guid> Handle(CreateSearchProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<SearchProfile>(request);
        entity.IsActive = true;

        _context.SearchProfiles.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
