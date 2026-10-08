using Klario.Common.Enums;

namespace Klario.Providers.Contracts;

public interface IPostingProvider
{
    PostingSource Source { get; }
    Task<IReadOnlyList<ExternalPosting>> FetchPostingsAsync(PostingSearchCriteria criteria, CancellationToken cancellationToken = default);
}
