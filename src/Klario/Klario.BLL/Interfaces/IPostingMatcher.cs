using Klario.Common.Enums;

namespace Klario.BLL.Interfaces;

public interface IPostingMatcher
{
    bool MatchesExperience(string postingTitle, IReadOnlyList<string> targetTitles, ExperienceLevel allowedExperience);
}
