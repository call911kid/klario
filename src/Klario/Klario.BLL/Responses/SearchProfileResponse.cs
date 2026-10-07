using Klario.DAL.Enums;

namespace Klario.BLL.Responses;

public record SearchProfileResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    List<string> TargetJobTitles,
    List<string> TargetLocations,
    TimeSpan MaxPostingAge,
    WorkplacePreference Workplace,
    ExperienceLevel Experience,
    JobType JobType,
    string TelegramChatId,
    DateTime CreatedAt
);
