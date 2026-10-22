using Klario.BLL.Commands.JobPostings;
using Klario.BLL.Interfaces;
using Klario.BLL.Queries.ExcludedCompanies;
using Klario.BLL.Queries.JobPostings;
using Klario.Common.Enums;
using Klario.DAL.Context;
using Klario.Providers.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Logging;

namespace Klario.BLL.Orchestrators;

public record PostingIngestionOrchestrator(
    Guid ProfileId,
    PostingSource Source = PostingSource.LinkedIn
) : IRequest;

public class PostingIngestionOrchestratorHandler : IRequestHandler<PostingIngestionOrchestrator>
{
    private readonly KlarioDbContext _context;
    private readonly IEnumerable<IPostingProvider> _providers;
    private readonly IMediator _mediator;
    private readonly IPostingAlertFormatter _formatter;
    private readonly ITelegramService _telegramService;
    private readonly IPostingMatcher _postingMatcher;
    private readonly ILogger<PostingIngestionOrchestratorHandler> _logger;

    public PostingIngestionOrchestratorHandler(
        KlarioDbContext context,
        IEnumerable<IPostingProvider> providers,
        IMediator mediator,
        IPostingAlertFormatter formatter,
        ITelegramService telegramService,
        IPostingMatcher postingMatcher,
        ILogger<PostingIngestionOrchestratorHandler> logger)
    {
        _context = context;
        _providers = providers;
        _mediator = mediator;
        _formatter = formatter;
        _telegramService = telegramService;
        _postingMatcher = postingMatcher;
        _logger = logger;
    }

    public async Task Handle(PostingIngestionOrchestrator request, CancellationToken cancellationToken)
    {
        var profile = await _context.SearchProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.Id == request.ProfileId, cancellationToken);

        if (profile is null || !profile.IsActive)
            return;

        var provider = _providers.FirstOrDefault(p => p.Source == request.Source);
        if (provider is null)
            return;

        int discoveredCount = 0;
        int matchedCount = 0;
        int existingCount = 0;
        int blockedCount = 0;
        int savedCount = 0;
        int deliveredCount = 0;
        int notDeliveredCount = 0;

        try
        {
            var criteria = new PostingSearchCriteria(
                TargetTitles: profile.TargetJobTitles,
                TargetLocations: profile.TargetLocations,
                MaxPostingAge: TimeSpan.FromMinutes(profile.MaxPostingAgeMinutes),
                Workplace: profile.Workplace,
                Experience: profile.Experience,
                JobType: profile.JobType
            );

            var discoveredPostings = await provider.FetchPostingsAsync(criteria, cancellationToken);
            discoveredCount = discoveredPostings.Count;
            if (discoveredCount == 0)
                return;

            var matchedPostings = discoveredPostings
                .Where(p => _postingMatcher.IsMatch(p.Title, profile.TargetJobTitles, profile.Experience))
                .ToList();
            matchedCount = matchedPostings.Count;
            if (matchedCount == 0)
                return;

            var externalIds = matchedPostings.Select(p => p.ExternalId).ToList();
            var existingIdsQuery = new GetExistingJobPostingIdsQuery(request.Source, externalIds);
            var existingIds = await _mediator.Send(existingIdsQuery, cancellationToken);
            existingCount = matchedPostings.Count(p => existingIds.Contains(p.ExternalId));

            var newPostings = matchedPostings
                .Where(p => !existingIds.Contains(p.ExternalId))
                .ToList();

            if (newPostings.Count == 0)
                return;

            var createCommand = new CreateJobPostingsCommand(newPostings);
            var insertedPostings = await _mediator.Send(createCommand, cancellationToken);
            savedCount = insertedPostings.Count;
            if (savedCount == 0)
                return;

            var candidateCompanies = insertedPostings
                .Select(p => p.Company)
                .Distinct()
                .ToList();

            var excludedCompaniesQuery = new GetMatchingExcludedCompaniesQuery(candidateCompanies);
            var excludedNames = await _mediator.Send(excludedCompaniesQuery, cancellationToken);

            var alertPostings = insertedPostings;
            if (excludedNames.Count > 0)
            {
                alertPostings = insertedPostings
                    .Where(p => !excludedNames.Contains(p.Company.Trim().ToLowerInvariant()))
                    .ToList();
                blockedCount = insertedPostings.Count - alertPostings.Count;
            }

            foreach (var posting in alertPostings)
            {
                try
                {
                    string message = _formatter.Format(posting);
                    await _telegramService.SendMessageAsync(
                        profile.BotToken,
                        profile.TelegramChatId,
                        message,
                        "HTML",
                        cancellationToken
                    );
                    deliveredCount++;
                }
                catch (Exception ex)
                {
                    notDeliveredCount++;
                    _logger.LogError(
                        ex,
                        "[{ProfileName}] [{Provider}] Failed to deliver Telegram alert for posting [{Title}] ({ExternalId}) to chat [{ChatId}].",
                        profile.Name,
                        request.Source,
                        posting.Title,
                        posting.ExternalId,
                        profile.TelegramChatId
                    );
                }
            }
        }
        finally
        {
            _logger.LogInformation(
                "[{ProfileName}] [{Provider}] Discovered: {DiscoveredCount} | Matched: {MatchedCount} | Existing: {ExistingCount} | Blocked: {BlockedCount} | Saved: {SavedCount} | Delivered: {DeliveredCount} | Not Delivered: {NotDeliveredCount}",
                profile.Name,
                request.Source,
                discoveredCount,
                matchedCount,
                existingCount,
                blockedCount,
                savedCount,
                deliveredCount,
                notDeliveredCount
            );
        }
    }
}
