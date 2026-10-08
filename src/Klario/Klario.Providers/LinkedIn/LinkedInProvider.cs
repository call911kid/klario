using System.Collections.Concurrent;
using HtmlAgilityPack;
using Klario.Common.Enums;
using Klario.Providers.Contracts;

namespace Klario.Providers.LinkedIn;

public class LinkedInProvider : IPostingProvider
{
    private const string BaseSearchUrl = "https://www.linkedin.com/jobs-guest/jobs/api/seeMoreJobPostings/search";
    private const string CanonicalPostingBaseUrl = "https://linkedin.com/jobs/view";

    private readonly HttpClient _httpClient;

    public PostingSource Source => PostingSource.LinkedIn;

    public LinkedInProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ExternalPosting>> FetchPostingsAsync(
        PostingSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        if (criteria.TargetTitles.Count == 0 || criteria.TargetLocations.Count == 0)
            return Array.Empty<ExternalPosting>();

        var targets = BuildSearchTargets(criteria.TargetTitles, criteria.TargetLocations);
        var discoveredPostings = await ExecuteParallelSearchesAsync(targets, criteria.MaxPostingAge, cancellationToken);

        return discoveredPostings
            .DistinctBy(p => p.ExternalId)
            .ToList()
            .AsReadOnly();
    }

    private static List<(string Title, string Location)> BuildSearchTargets(
        IReadOnlyList<string> titles,
        IReadOnlyList<string> locations)
    {
        return titles
            .SelectMany(title => locations.Select(location => (Title: title, Location: location)))
            .ToList();
    }

    private async Task<List<ExternalPosting>> ExecuteParallelSearchesAsync(
        List<(string Title, string Location)> targets,
        TimeSpan maxPostingAge,
        CancellationToken cancellationToken)
    {
        var discoveredPostings = new ConcurrentBag<ExternalPosting>();

        await Parallel.ForEachAsync(
            targets,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = 3,
                CancellationToken = cancellationToken
            },
            async (target, ct) =>
            {
                try
                {
                    var postings = await FetchPostingsForTargetAsync(target.Title, target.Location, maxPostingAge, ct);
                    foreach (var posting in postings)
                    {
                        discoveredPostings.Add(posting);
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch
                {
                    // Ignore single target failure to allow remaining searches to complete.
                }

                await Task.Delay(TimeSpan.FromSeconds(2), ct);
            });

        return discoveredPostings.ToList();
    }

    private async Task<List<ExternalPosting>> FetchPostingsForTargetAsync(
        string title,
        string location,
        TimeSpan maxPostingAge,
        CancellationToken cancellationToken)
    {
        string searchUrl = BuildSearchUrl(title, location, maxPostingAge);
        string? html = await FetchHtmlAsync(searchUrl, cancellationToken);

        if (string.IsNullOrWhiteSpace(html))
            return new List<ExternalPosting>();

        return ParsePostingCards(html);
    }

    private static string BuildSearchUrl(string title, string location, TimeSpan maxPostingAge)
    {
        int seconds = (int)maxPostingAge.TotalSeconds;
        if (seconds <= 0) seconds = 3600;

        return $"{BaseSearchUrl}?keywords={Uri.EscapeDataString(title)}&location={Uri.EscapeDataString(location)}&f_TPR=r{seconds}&sortBy=DD&start=0";
    }

    private async Task<string?> FetchHtmlAsync(string searchUrl, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(searchUrl, cancellationToken);

        if ((int)response.StatusCode == 429 || !response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static List<ExternalPosting> ParsePostingCards(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var cardNodes = doc.DocumentNode.SelectNodes("//*[contains(@class,'base-card')]")
                        ?? doc.DocumentNode.SelectNodes("//*[.//a[contains(@href,'/jobs/view/')]]");

        if (cardNodes == null || cardNodes.Count == 0)
            return new List<ExternalPosting>();

        var postings = new List<ExternalPosting>();

        foreach (var node in cardNodes)
        {
            var posting = ParsePostingCard(node);
            if (posting != null)
            {
                postings.Add(posting);
            }
        }

        return postings;
    }

    private static ExternalPosting? ParsePostingCard(HtmlNode node)
    {
        string externalId = ExtractExternalId(node);
        if (string.IsNullOrWhiteSpace(externalId))
            return null;

        string title = CleanText(node.SelectSingleNode(".//h3")?.InnerText);
        string company = CleanText(node.SelectSingleNode(".//h4")?.InnerText);
        string? companyIdentifier = ExtractCompanyIdentifier(node);
        string location = CleanText(node.SelectSingleNode(".//*[contains(@class,'job-search-card__location')]")?.InnerText);
        string postedText = CleanText(node.SelectSingleNode(".//*[contains(@class,'job-search-card__listdate')]")?.InnerText);
        DateTime? postedAt = ExtractPostedDate(node);
        string canonicalUrl = BuildCanonicalPostingUrl(externalId);

        return new ExternalPosting(
            ExternalId: externalId,
            Title: title,
            Company: company,
            CompanyIdentifier: companyIdentifier,
            Location: location,
            Url: canonicalUrl,
            PostedText: postedText,
            PostedAt: postedAt,
            Source: PostingSource.LinkedIn
        );
    }

    private static string BuildCanonicalPostingUrl(string externalId)
    {
        return $"{CanonicalPostingBaseUrl}/{externalId}";
    }

    private static string ExtractExternalId(HtmlNode node)
    {
        string urn = node.GetAttributeValue("data-entity-urn", "");
        if (!string.IsNullOrWhiteSpace(urn))
        {
            int colonIndex = urn.LastIndexOf(':');
            if (colonIndex >= 0)
                return urn[(colonIndex + 1)..];
        }

        string href = node.SelectSingleNode(".//a[contains(@href,'/jobs/view/')]")?.GetAttributeValue("href", "") ?? "";
        if (string.IsNullOrWhiteSpace(href))
            return string.Empty;

        var parts = href.Split('/', StringSplitOptions.RemoveEmptyEntries);
        int jobsIndex = Array.IndexOf(parts, "jobs");
        if (jobsIndex >= 0 && jobsIndex + 2 < parts.Length)
        {
            string potentialIdWithSlug = parts[jobsIndex + 2];
            int lastDashIndex = potentialIdWithSlug.LastIndexOf('-');
            return lastDashIndex >= 0 ? potentialIdWithSlug[(lastDashIndex + 1)..] : potentialIdWithSlug;
        }

        return string.Empty;
    }

    private static string? ExtractCompanyIdentifier(HtmlNode node)
    {
        var companyLinkNode = node.SelectSingleNode(".//h4//a")
                              ?? node.SelectSingleNode(".//*[contains(@class,'base-search-card__subtitle')]//a");

        string href = companyLinkNode?.GetAttributeValue("href", "") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(href))
            return null;

        int queryIndex = href.IndexOf('?');
        if (queryIndex >= 0)
            href = href[..queryIndex];

        var parts = href.Split('/', StringSplitOptions.RemoveEmptyEntries);
        int companyIndex = Array.IndexOf(parts, "company");
        if (companyIndex >= 0 && companyIndex + 1 < parts.Length)
        {
            return parts[companyIndex + 1].Trim().ToLowerInvariant();
        }

        return null;
    }

    private static DateTime? ExtractPostedDate(HtmlNode node)
    {
        var timeNode = node.SelectSingleNode(".//time");
        string datetimeAttr = timeNode?.GetAttributeValue("datetime", "") ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(datetimeAttr) && DateTime.TryParse(datetimeAttr, out var parsedDate))
        {
            return parsedDate;
        }

        return null;
    }

    private static string CleanText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return HtmlEntity.DeEntitize(text)
            .Trim()
            .Replace("\n", " ")
            .Replace("\r", " ")
            .Replace("\t", " ");
    }
}
