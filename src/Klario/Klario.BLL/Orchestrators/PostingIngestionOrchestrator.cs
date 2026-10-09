using Klario.BLL.Commands.JobPostings;
using Klario.BLL.Interfaces;
using Klario.BLL.Queries.JobPostings;
using Klario.Common.Enums;
using Klario.DAL.Context;
using Klario.Providers.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

    public PostingIngestionOrchestratorHandler(
        KlarioDbContext context,
        IEnumerable<IPostingProvider> providers,
        IMediator mediator,
        IPostingAlertFormatter formatter,
        ITelegramService telegramService)
    {
        _context = context;
        _providers = providers;
        _mediator = mediator;
        _formatter = formatter;
        _telegramService = telegramService;
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

        var criteria = new PostingSearchCriteria(
            TargetTitles: profile.TargetJobTitles,
            TargetLocations: profile.TargetLocations,
            MaxPostingAge: TimeSpan.FromMinutes(profile.MaxPostingAgeMinutes),
            Workplace: profile.Workplace,
            Experience: profile.Experience,
            JobType: profile.JobType
        );

        var discoveredPostings = await provider.FetchPostingsAsync(criteria, cancellationToken);
        if (discoveredPostings.Count == 0)
            return;

        var externalIds = discoveredPostings.Select(p => p.ExternalId).ToList();
        var existingIdsQuery = new GetExistingJobPostingIdsQuery(request.Source, externalIds);
        var existingIds = await _mediator.Send(existingIdsQuery, cancellationToken);

        var newPostings = discoveredPostings
            .Where(p => !existingIds.Contains(p.ExternalId))
            .ToList();

        if (newPostings.Count == 0)
            return;

        var createCommand = new CreateJobPostingsCommand(newPostings);
        await _mediator.Send(createCommand, cancellationToken);

        foreach (var posting in newPostings)
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
            }
            catch
            {
                // Ignore individual delivery failure so remaining alerts can proceed
            }
        }
    }
}
