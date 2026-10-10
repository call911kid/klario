using System.Text.RegularExpressions;
using Klario.BLL.Interfaces;
using Klario.Common.Enums;

namespace Klario.BLL.Services;

public class PostingMatcher : IPostingMatcher
{
    private record SeniorityRule(ExperienceLevel Level, Regex Pattern);

    private static readonly SeniorityRule[] SeniorityExclusions =
    [
        new(ExperienceLevel.Lead,       new(@"\b(lead|principal|architect|director|head|vp|manager|staff)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        new(ExperienceLevel.Senior,     new(@"\b(sr\.?|senior)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        new(ExperienceLevel.Junior,     new(@"\b(jr\.?|junior|entry|associate|graduate|fresh|trainee)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        new(ExperienceLevel.Internship, new(@"\b(intern|internship|co-?op)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
    ];

    private static readonly Regex SeparatorsRegex = new(@"[-/_|\\()\[\]{},:;~!]+", RegexOptions.Compiled);
    private static readonly Regex MultiSpaceRegex = new(@"\s+", RegexOptions.Compiled);

    public bool IsMatch(
        string postingTitle,
        IReadOnlyList<string> targetTitles,
        ExperienceLevel allowedExperience)
    {
        if (string.IsNullOrWhiteSpace(postingTitle))
            return false;

        if (!MatchesExperience(postingTitle, targetTitles, allowedExperience))
            return false;

        if (!MatchesTitle(postingTitle, targetTitles))
            return false;

        return true;
    }

    public bool MatchesExperience(
        string postingTitle,
        IReadOnlyList<string> targetTitles,
        ExperienceLevel allowedExperience)
    {
        if (allowedExperience == ExperienceLevel.None)
            return true;

        foreach (var rule in SeniorityExclusions)
        {
            if (rule.Pattern.IsMatch(postingTitle))
            {
                bool isTargeted = targetTitles.Any(t => rule.Pattern.IsMatch(t));
                if (!allowedExperience.HasFlag(rule.Level) && !isTargeted)
                    return false;
            }
        }

        return true;
    }

    public bool MatchesTitle(string postingTitle, IReadOnlyList<string> targetTitles)
    {
        if (targetTitles.Count == 0)
            return true;

        string sanitizedPosting = Sanitize(postingTitle);

        return targetTitles.Any(target =>
        {
            if (string.IsNullOrWhiteSpace(target))
                return false;

            string sanitizedTarget = Sanitize(target);
            return sanitizedPosting.Contains(sanitizedTarget, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static string Sanitize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        string withSpaces = SeparatorsRegex.Replace(text, " ");
        return MultiSpaceRegex.Replace(withSpaces, " ").Trim();
    }
}
