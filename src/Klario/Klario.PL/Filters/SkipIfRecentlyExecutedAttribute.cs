using System.Globalization;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using Klario.Common.Enums;
using Klario.PL.Services;

namespace Klario.PL.Filters;

public sealed class SkipIfRecentlyExecutedAttribute : JobFilterAttribute, IServerFilter
{
    public void OnPerforming(PerformingContext filterContext)
    {
        var args = filterContext.BackgroundJob.Job.Args;
        if (args.Count < 3)
            return;

        if (!Guid.TryParse(args[0]?.ToString(), out var profileId) ||
            !Enum.TryParse<PostingSource>(args[1]?.ToString(), out var source) ||
            !int.TryParse(args[2]?.ToString(), out int intervalMinutes) ||
            intervalMinutes <= 0)
        {
            return;
        }

        string recurringJobId = HangfirePostingScheduleManager.GetJobId(profileId, source);
        string throttleKey = $"throttle:{recurringJobId}";

        var entries = filterContext.Connection.GetAllEntriesFromHash(throttleKey);
        if (entries is not null && 
            entries.TryGetValue("LastRun", out var lastRunStr) && 
            DateTime.TryParse(lastRunStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var lastRun))
        {
            var elapsed = DateTime.UtcNow - lastRun;
            var minInterval = TimeSpan.FromMinutes(intervalMinutes * 0.8);

            if (elapsed < minInterval)
            {
                filterContext.Canceled = true;
                return;
            }
        }

        filterContext.Connection.SetRangeInHash(throttleKey, new[]
        {
            new KeyValuePair<string, string>("LastRun", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture))
        });
    }

    public void OnPerformed(PerformedContext filterContext)
    {
    }
}
