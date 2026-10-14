using Klario.Common.Enums;

namespace Klario.PL.Contracts.SearchProfiles;

public sealed record SearchProfileResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    List<string> TargetJobTitles,
    List<string> TargetLocations,
    int MaxPostingAgeMinutes,
    int IntervalMinutes,
    WorkplacePreference Workplace,
    ExperienceLevel Experience,
    JobType JobType,
    string TelegramChatId,
    string BotToken,
    DateTime CreatedAt
);
