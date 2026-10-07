using Klario.DAL.Enums;

namespace Klario.PL.DTOs.Responses;

public class SearchProfileResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<string> TargetJobTitles { get; set; } = new();
    public List<string> TargetLocations { get; set; } = new();
    public TimeSpan MaxPostingAge { get; set; }
    public WorkplacePreference Workplace { get; set; }
    public ExperienceLevel Experience { get; set; }
    public JobType JobType { get; set; }
    public string TelegramChatId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
