using Klario.Common.Enums;

namespace Klario.Providers.Contracts;

public record PostingSearchCriteria(
    IReadOnlyList<string> TargetTitles,
    IReadOnlyList<string> TargetLocations,
    TimeSpan MaxPostingAge,
    WorkplacePreference Workplace = WorkplacePreference.None,
    ExperienceLevel Experience = ExperienceLevel.None,
    JobType JobType = JobType.None
);
