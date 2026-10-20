using Klario.Common.Enums;
using System.Text.Json.Serialization;

namespace Klario.Console.Models;

public sealed class SearchProfileViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> TargetJobTitles { get; set; } = [];
    public List<string> TargetLocations { get; set; } = [];
    public int MaxPostingAgeMinutes { get; set; } = 1440;
    public WorkplacePreference Workplace { get; set; } = WorkplacePreference.Remote;
    public ExperienceLevel Experience { get; set; } = ExperienceLevel.MidLevel | ExperienceLevel.Senior;
    public JobType JobType { get; set; } = JobType.FullTime;

    public string TelegramChatId { get; set; } = string.Empty;
    public string BotToken { get; set; } = string.Empty;
    public List<ProviderScheduleModel> Schedules { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string FormatCreatedAt()
    {
        var span = DateTime.UtcNow - CreatedAt;
        if (span.TotalDays >= 365) return $"{(int)(span.TotalDays / 365)}y old";
        if (span.TotalDays >= 30) return $"{(int)(span.TotalDays / 30)}mo old";
        if (span.TotalDays >= 1) return $"{(int)span.TotalDays}d old";
        if (span.TotalHours >= 1) return $"{(int)span.TotalHours}h old";
        if (span.TotalMinutes >= 1) return $"{(int)span.TotalMinutes}m old";
        return "just now";
    }

    public bool HasWorkplace(WorkplacePreference flag) => (Workplace & flag) == flag;
    public void ToggleWorkplace(WorkplacePreference flag)
    {
        if (HasWorkplace(flag))
            Workplace &= ~flag;
        else
            Workplace |= flag;
    }
    public void SelectAllWorkplace() => Workplace = WorkplacePreference.Remote | WorkplacePreference.Hybrid | WorkplacePreference.Onsite;
    public void ClearWorkplace() => Workplace = WorkplacePreference.None;

    public bool HasExperience(ExperienceLevel flag) => (Experience & flag) == flag;
    public void ToggleExperience(ExperienceLevel flag)
    {
        if (HasExperience(flag))
            Experience &= ~flag;
        else
            Experience |= flag;
    }
    public void SelectAllExperience() => Experience = ExperienceLevel.Internship | ExperienceLevel.Junior | ExperienceLevel.MidLevel | ExperienceLevel.Senior | ExperienceLevel.Lead;
    public void ClearExperience() => Experience = ExperienceLevel.None;

    public bool HasJobType(JobType flag) => (JobType & flag) == flag;
    public void ToggleJobType(JobType flag)
    {
        if (HasJobType(flag))
            JobType &= ~flag;
        else
            JobType |= flag;
    }
    public void SelectAllJobType() => JobType = JobType.FullTime | JobType.PartTime | JobType.Contract | JobType.Internship;
    public void ClearJobType() => JobType = JobType.None;
}

public sealed class ExcludedCompanyModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class ApiEnvelope<T>
{
    public bool IsSuccess { get; set; }
    public string? ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}

public sealed class ProviderScheduleModel
{
    public PostingSource Source { get; set; } = PostingSource.LinkedIn;
    public string Name { get; set; } = "LinkedIn Jobs";
    public string Subtitle { get; set; } = "Guest HTTP Ingestion";
    public int IntervalMinutes { get; set; } = 15;
    public bool IsEnabled { get; set; } = true;
}

public sealed record ScheduleDto(PostingSource Source, int IntervalMinutes, bool IsEnabled);

public sealed record SearchProfileApiDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    List<string> TargetJobTitles,
    List<string> TargetLocations,
    int MaxPostingAgeMinutes,
    List<ScheduleDto> Schedules,
    WorkplacePreference Workplace,
    ExperienceLevel Experience,
    JobType JobType,
    string TelegramChatId,
    string BotToken,
    DateTime CreatedAt
);

public sealed record JobPostingStatsModel(int TodayCount, int WeekCount, int TotalCount);

public sealed record JobPostingDto(
    Guid Id,
    string ExternalJobId,
    string Title,
    string Company,
    string Location,
    string Url,
    DateTime? PostedAt,
    PostingSource Source,
    string? DescriptionSnippet,
    DateTime CreatedAt,
    bool IsCompanyBlocked = false
);
