namespace Klario.BLL.Interfaces;

public interface IPostingScheduleManager
{
    void ScheduleRecurringIngestion(Guid profileId, string cronExpression = "*/15 * * * *");
    void RemoveRecurringIngestion(Guid profileId);
}
