namespace Klario.BLL.Interfaces;

public interface IPostingScheduleManager
{
    void ScheduleRecurringIngestion(Guid profileId, int intervalMinutes);
    void RemoveRecurringIngestion(Guid profileId);
}
