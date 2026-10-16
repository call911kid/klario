using Klario.BLL.Exceptions;
using Klario.BLL.Interfaces;
using Klario.DAL.Context;
using Klario.DAL.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Commands.SearchProfiles;

public sealed record SetSearchProfileStatusCommand(
    Guid Id,
    bool IsActive
) : IRequest;

public class SetSearchProfileStatusCommandHandler : IRequestHandler<SetSearchProfileStatusCommand>
{
    private readonly KlarioDbContext _context;
    private readonly IPostingScheduleManager _scheduleManager;

    public SetSearchProfileStatusCommandHandler(
        KlarioDbContext context,
        IPostingScheduleManager scheduleManager)
    {
        _context = context;
        _scheduleManager = scheduleManager;
    }

    public async Task Handle(SetSearchProfileStatusCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.SearchProfiles
            .Include(sp => sp.Schedules)
            .FirstOrDefaultAsync(sp => sp.Id == request.Id, cancellationToken);

        if (profile is null)
        {
            throw new EntityNotFoundException(nameof(SearchProfile), request.Id);
        }

        profile.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);

        if (profile.IsActive && !profile.IsDeleted)
        {
            foreach (var schedule in profile.Schedules.Where(s => s.IsEnabled))
            {
                _scheduleManager.ScheduleRecurringIngestion(profile.Id, schedule.Source, schedule.IntervalMinutes);
            }
        }
        else
        {
            foreach (var schedule in profile.Schedules)
            {
                _scheduleManager.RemoveRecurringIngestion(profile.Id, schedule.Source);
            }
        }
    }
}
