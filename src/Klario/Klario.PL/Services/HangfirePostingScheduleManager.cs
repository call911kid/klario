using Hangfire;
using Klario.BLL.Interfaces;
using Klario.BLL.Orchestrators;
using Klario.Common.Enums;
using MediatR;

namespace Klario.PL.Services;

public class HangfirePostingScheduleManager : IPostingScheduleManager
{
    private const string DefaultCronExpression = "*/15 * * * *";

    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IMediator _mediator;

    public HangfirePostingScheduleManager(IRecurringJobManager recurringJobManager, IMediator mediator)
    {
        _recurringJobManager = recurringJobManager;
        _mediator = mediator;
    }

    public void ScheduleRecurringIngestion(Guid profileId, string cronExpression = DefaultCronExpression)
    {
        string jobId = GetJobId(profileId);
        string cron = string.IsNullOrWhiteSpace(cronExpression) ? DefaultCronExpression : cronExpression;

        _recurringJobManager.AddOrUpdate<HangfirePostingScheduleManager>(
            jobId,
            manager => manager.ExecuteIngestionAsync(profileId),
            cron
        );
    }

    public void RemoveRecurringIngestion(Guid profileId)
    {
        string jobId = GetJobId(profileId);
        _recurringJobManager.RemoveIfExists(jobId);
    }

    public async Task ExecuteIngestionAsync(Guid profileId)
    {
        await _mediator.Send(new PostingIngestionOrchestrator(profileId, PostingSource.LinkedIn));
    }

    private static string GetJobId(Guid profileId) => $"posting-ingestion-{profileId}";
}
