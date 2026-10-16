using Klario.BLL.Exceptions;
using Klario.DAL.Context;
using Klario.Common.Enums;
using Klario.DAL.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Commands.SearchProfiles;

public record UpdateSearchProfileCommand(
    Guid Id,
    string Name,
    string? Description,
    List<string> TargetJobTitles,
    List<string> TargetLocations,
    int MaxPostingAgeMinutes,
    WorkplacePreference Workplace,
    ExperienceLevel Experience,
    JobType JobType,
    string TelegramChatId,
    string BotToken
) : IRequest;

public class UpdateSearchProfileCommandHandler : IRequestHandler<UpdateSearchProfileCommand>
{
    private readonly KlarioDbContext _context;

    public UpdateSearchProfileCommandHandler(KlarioDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateSearchProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SearchProfiles
            .FirstOrDefaultAsync(sp => sp.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new EntityNotFoundException(nameof(SearchProfile), request.Id);
        }

        entity.Name = request.Name;
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.TargetJobTitles = request.TargetJobTitles;
        entity.TargetLocations = request.TargetLocations;
        entity.MaxPostingAgeMinutes = request.MaxPostingAgeMinutes;
        entity.Workplace = request.Workplace;
        entity.Experience = request.Experience;
        entity.JobType = request.JobType;
        entity.TelegramChatId = request.TelegramChatId;

        if (!string.IsNullOrWhiteSpace(request.BotToken))
        {
            entity.BotToken = request.BotToken.Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
