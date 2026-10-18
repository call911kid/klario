using FluentValidation;
using Klario.BLL.Exceptions;
using Klario.BLL.Interfaces;
using Klario.Common.Enums;
using Klario.DAL.Context;
using Klario.DAL.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Commands.Schedules;

public sealed record ConfigureScheduleCommand(
    Guid SearchProfileId,
    PostingSource Source,
    int IntervalMinutes
) : IRequest;

public class ConfigureScheduleCommandHandler : IRequestHandler<ConfigureScheduleCommand>
{
    private readonly KlarioDbContext _context;
    private readonly IPostingScheduleManager _scheduleManager;

    public ConfigureScheduleCommandHandler(
        KlarioDbContext context,
        IPostingScheduleManager scheduleManager)
    {
        _context = context;
        _scheduleManager = scheduleManager;
    }

    public async Task Handle(ConfigureScheduleCommand request, CancellationToken cancellationToken)
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
            schedule = new Schedule
            {
                SearchProfileId = profile.Id,
                Source = request.Source,
                IntervalMinutes = request.IntervalMinutes,
                IsEnabled = true
            };
            profile.Schedules.Add(schedule);
        }
        else
        {
            schedule.IntervalMinutes = request.IntervalMinutes;
            schedule.IsEnabled = true;
        }

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

public class ConfigureScheduleCommandValidator : AbstractValidator<ConfigureScheduleCommand>
{
    public ConfigureScheduleCommandValidator()
    {
        RuleFor(x => x.SearchProfileId)
            .NotEmpty().WithMessage("SearchProfileId is required.");

        RuleFor(x => x.Source)
            .IsInEnum().WithMessage("Invalid posting source value.");

        RuleFor(x => x.IntervalMinutes)
            .GreaterThan(0).WithMessage("IntervalMinutes must be greater than 0.");
    }
}
