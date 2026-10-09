using Klario.Common.Enums;

namespace Klario.PL.DTOs.Requests;

public class UpdateSearchProfileRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> TargetJobTitles { get; set; } = new();
    public List<string> TargetLocations { get; set; } = new();
    public int MaxPostingAgeMinutes { get; set; } = 1440;
    public int IntervalMinutes { get; set; } = 15;
    public WorkplacePreference Workplace { get; set; } = WorkplacePreference.Remote;
    public ExperienceLevel Experience { get; set; } = ExperienceLevel.MidLevel;
    public JobType JobType { get; set; } = JobType.FullTime;
    public string TelegramChatId { get; set; } = string.Empty;
    public string BotToken { get; set; } = string.Empty;
}
