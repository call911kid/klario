using Hangfire;
using Hangfire.Storage;
using Klario.BLL.Interfaces;
using Klario.BLL.Orchestrators;
using Klario.BLL.Queries.Schedules;
using Klario.Common.Enums;
using Klario.PL.Filters;
using MediatR;

namespace Klario.PL.Services;

public class HangfirePostingScheduleManager : IPostingScheduleManager
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IMediator _mediator;

    public HangfirePostingScheduleManager(
        IRecurringJobManager recurringJobManager,
        IMediator mediator)
    {
        _recurringJobManager = recurringJobManager;
        _mediator = mediator;
    }

    public void ScheduleRecurringIngestion(Guid profileId, PostingSource source, int intervalMinutes)
    {
        string jobId = GetJobId(profileId, source);
        string cron = intervalMinutes >= 60
            ? Cron.HourInterval(Math.Max(1, intervalMinutes / 60))
            : Cron.MinuteInterval(Math.Max(1, intervalMinutes));

        _recurringJobManager.AddOrUpdate<HangfirePostingScheduleManager>(
            jobId,
            manager => manager.ExecuteIngestionAsync(profileId, source, intervalMinutes),
            cron
        );
    }

    public void RemoveRecurringIngestion(Guid profileId, PostingSource source)
    {
        string jobId = GetJobId(profileId, source);
        _recurringJobManager.RemoveIfExists(jobId);
    }

    [SkipIfRecentlyExecuted]
    public async Task ExecuteIngestionAsync(Guid profileId, PostingSource source, int intervalMinutes)
    {
        await _mediator.Send(new PostingIngestionOrchestrator(profileId, source));
    }

    public async Task RescheduleAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = JobStorage.Current.GetConnection();
        var recurringJobs = connection.GetRecurringJobs();
        foreach (var job in recurringJobs)
        {
            _recurringJobManager.RemoveIfExists(job.Id);
        }

        var activeSchedules = await _mediator.Send(new GetActiveSchedulesQuery(), cancellationToken);

        foreach (var schedule in activeSchedules)
        {
            ScheduleRecurringIngestion(schedule.ProfileId, schedule.Source, schedule.IntervalMinutes);
        }
    }

    public static string GetJobId(Guid profileId, PostingSource source) => $"ingest:{profileId}:{source}";
}
