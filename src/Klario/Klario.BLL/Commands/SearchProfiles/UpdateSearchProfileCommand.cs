using AutoMapper;
using Klario.BLL.Exceptions;
using Klario.DAL.Context;
using Klario.DAL.Enums;
using Klario.DAL.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Commands.SearchProfiles;

public record UpdateSearchProfileCommand(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    List<string> TargetJobTitles,
    List<string> TargetLocations,
    TimeSpan MaxPostingAge,
    WorkplacePreference Workplace,
    ExperienceLevel Experience,
    JobType JobType,
    string TelegramChatId,
    string BotToken
) : IRequest;

public class UpdateSearchProfileCommandHandler : IRequestHandler<UpdateSearchProfileCommand>
{
    private readonly KlarioDbContext _context;
    private readonly IMapper _mapper;

    public UpdateSearchProfileCommandHandler(KlarioDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task Handle(UpdateSearchProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SearchProfiles
            .FirstOrDefaultAsync(sp => sp.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new EntityNotFoundException(nameof(SearchProfile), request.Id);
        }

        _mapper.Map(request, entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
