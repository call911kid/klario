using Klario.BLL.Exceptions;
using Klario.BLL.Interfaces;
using Klario.Common.Enums;
using Klario.DAL.Context;
using Klario.DAL.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Commands.Schedules;

public sealed record SetScheduleStatusCommand(
    Guid SearchProfileId,
    PostingSource Source,
    bool IsEnabled
) : IRequest;

public class SetScheduleStatusCommandHandler : IRequestHandler<SetScheduleStatusCommand>
{
    private readonly KlarioDbContext _context;
    private readonly IPostingScheduleManager _scheduleManager;

    public SetScheduleStatusCommandHandler(
        KlarioDbContext context,
        IPostingScheduleManager scheduleManager)
    {
        _context = context;
        _scheduleManager = scheduleManager;
    }

    public async Task Handle(SetScheduleStatusCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.SearchProfiles
            .Include(sp => sp.Schedules)
            .FirstOrDefaultAsync(sp => sp.Id == request.SearchProfileId, cancellationToken);

        if (profile is null)
        {
            throw new EntityNotFoundException(nameof(SearchProfile), request.SearchProfileId);
        }

        var schedule = profile.Schedules.FirstOrDefault(s => s.Source == request.Source);
        if (schedule is null)
        {
            throw new EntityNotFoundException($"Schedule for source '{request.Source}' on profile '{request.SearchProfileId}' not found.");
        }

        schedule.IsEnabled = request.IsEnabled;
        await _context.SaveChangesAsync(cancellationToken);

        if (profile.IsActive && !profile.IsDeleted && schedule.IsEnabled)
        {
            _scheduleManager.ScheduleRecurringIngestion(profile.Id, schedule.Source, schedule.IntervalMinutes);
        }
        else
        {
            _scheduleManager.RemoveRecurringIngestion(profile.Id, schedule.Source);
        }
    }
}
