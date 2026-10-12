using Klario.BLL.Exceptions;
using Klario.BLL.Interfaces;
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
    bool IsActive,
    List<string> TargetJobTitles,
    List<string> TargetLocations,
    int MaxPostingAgeMinutes,
    int IntervalMinutes,
    WorkplacePreference Workplace,
    ExperienceLevel Experience,
    JobType JobType,
    string TelegramChatId,
    string BotToken
) : IRequest;

public class UpdateSearchProfileCommandHandler : IRequestHandler<UpdateSearchProfileCommand>
{
    private readonly KlarioDbContext _context;
    private readonly IPostingScheduleManager _scheduleManager;

    public UpdateSearchProfileCommandHandler(
        KlarioDbContext context,
        IPostingScheduleManager scheduleManager)
    {
        _context = context;
        _scheduleManager = scheduleManager;
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
        entity.IsActive = request.IsActive;
        entity.TargetJobTitles = request.TargetJobTitles;
        entity.TargetLocations = request.TargetLocations;
        entity.MaxPostingAgeMinutes = request.MaxPostingAgeMinutes;
        entity.IntervalMinutes = request.IntervalMinutes;
        entity.Workplace = request.Workplace;
        entity.Experience = request.Experience;
        entity.JobType = request.JobType;
        entity.TelegramChatId = request.TelegramChatId;

        if (!string.IsNullOrWhiteSpace(request.BotToken))
        {
            entity.BotToken = request.BotToken.Trim();
        }

        var scheduleChanged = _context.Entry(entity).Property(e => e.IntervalMinutes).IsModified ||
                              _context.Entry(entity).Property(e => e.IsActive).IsModified;

        await _context.SaveChangesAsync(cancellationToken);

        if (scheduleChanged)
        {
            if (entity.IsActive && !entity.IsDeleted)
            {
                _scheduleManager.ScheduleRecurringIngestion(entity.Id, entity.IntervalMinutes);
            }
            else
            {
                _scheduleManager.RemoveRecurringIngestion(entity.Id);
            }
        }
    }
}
