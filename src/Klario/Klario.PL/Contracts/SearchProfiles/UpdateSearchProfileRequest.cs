using Klario.Common.Enums;

namespace Klario.PL.Contracts.SearchProfiles;

public sealed record UpdateSearchProfileRequest(
    string Name,
    string? Description,
    List<string> TargetJobTitles,
    List<string> TargetLocations,
    int MaxPostingAgeMinutes = 1440,
    WorkplacePreference Workplace = WorkplacePreference.Remote,
    ExperienceLevel Experience = ExperienceLevel.MidLevel,
    JobType JobType = JobType.FullTime,
    string TelegramChatId = "",
    string BotToken = ""
);
