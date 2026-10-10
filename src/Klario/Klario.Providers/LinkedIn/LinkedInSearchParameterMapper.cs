using Klario.Common.Enums;

namespace Klario.Providers.LinkedIn;

internal static class LinkedInSearchParameterMapper
{
    public static string? MapExperience(ExperienceLevel experience)
    {
        if (experience == ExperienceLevel.None)
            return null;

        var values = new HashSet<string>();

        if (experience.HasFlag(ExperienceLevel.Internship))
            values.Add("1");

        if (experience.HasFlag(ExperienceLevel.Junior))
        {
            values.Add("2");
            values.Add("3");
        }

        if (experience.HasFlag(ExperienceLevel.MidLevel) || experience.HasFlag(ExperienceLevel.Senior))
            values.Add("4");

        if (experience.HasFlag(ExperienceLevel.Lead))
            values.Add("5");

        return values.Count != 0 ? string.Join(",", values) : null;
    }

    public static string? MapWorkplace(WorkplacePreference workplace)
    {
        if (workplace == WorkplacePreference.None)
            return null;

        var values = new List<string>();

        if (workplace.HasFlag(WorkplacePreference.Onsite))
            values.Add("1");

        if (workplace.HasFlag(WorkplacePreference.Remote))
            values.Add("2");

        if (workplace.HasFlag(WorkplacePreference.Hybrid))
            values.Add("3");

        return values.Count != 0 ? string.Join(",", values) : null;
    }

    public static string? MapJobType(JobType jobType)
    {
        if (jobType == JobType.None)
            return null;

        var values = new List<string>();

        if (jobType.HasFlag(JobType.FullTime))
            values.Add("F");

        if (jobType.HasFlag(JobType.PartTime))
            values.Add("P");

        if (jobType.HasFlag(JobType.Contract))
            values.Add("C");

        if (jobType.HasFlag(JobType.Internship))
            values.Add("I");

        return values.Count != 0 ? string.Join(",", values) : null;
    }
}
