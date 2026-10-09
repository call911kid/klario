using Klario.Common.Enums;

namespace Klario.PL.DTOs.Requests;

public class CreateSearchProfileRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> TargetJobTitles { get; set; } = new();
    public List<string> TargetLocations { get; set; } = new();
    public TimeSpan MaxPostingAge { get; set; } = TimeSpan.FromHours(2);
    public int IntervalMinutes { get; set; } = 15;
    public WorkplacePreference Workplace { get; set; } = WorkplacePreference.Remote;
    public ExperienceLevel Experience { get; set; } = ExperienceLevel.MidLevel;
    public JobType JobType { get; set; } = JobType.FullTime;
    public string TelegramChatId { get; set; } = string.Empty;
    public string BotToken { get; set; } = string.Empty;
}
