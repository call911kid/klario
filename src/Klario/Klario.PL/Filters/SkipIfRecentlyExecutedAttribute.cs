using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Storage;
using Klario.Common.Enums;
using Klario.PL.Services;

namespace Klario.PL.Filters;

public sealed class SkipIfRecentlyExecutedAttribute : JobFilterAttribute, IClientFilter
{
    public void OnCreating(CreatingContext filterContext)
    {
        var args = filterContext.Job.Args;
        if (args.Count < 2)
            return;

        if (!Guid.TryParse(args[0]?.ToString(), out var profileId) ||
            !Enum.TryParse<PostingSource>(args[1]?.ToString(), out var source))
        {
            return;
        }

        string recurringJobId = HangfirePostingScheduleManager.GetJobId(profileId, source);

        var recurringJob = filterContext.Connection
            .GetRecurringJobs()
            .FirstOrDefault(r => r.Id == recurringJobId);

        if (recurringJob?.LastJobId is not null)
        {
            var lastJobData = filterContext.Connection.GetJobData(recurringJob.LastJobId);
            if (lastJobData is not null && (lastJobData.State == "Enqueued"))
            {
                filterContext.Canceled = true;
                return;
            }
        }
    }

    public void OnCreated(CreatedContext filterContext)
    {
    }
}
