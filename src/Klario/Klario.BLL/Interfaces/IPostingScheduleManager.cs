using Klario.Common.Enums;

namespace Klario.BLL.Interfaces;

public interface IPostingScheduleManager
{
    void ScheduleRecurringIngestion(Guid profileId, PostingSource source, int intervalMinutes);
    void RemoveRecurringIngestion(Guid profileId, PostingSource source);
    Task SyncSchedulesAsync(CancellationToken cancellationToken = default);
}
