using FluentValidation;
using Klario.BLL.Exceptions;
using Klario.DAL.Context;
using Klario.Common.Enums;
using Klario.DAL.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Commands.SearchProfiles;

public record UpdateSearchProfileCommand(
    Guid Id,
    string Name,
    string? Description,
    List<string> TargetJobTitles,
    List<string> TargetLocations,
    int MaxPostingAgeMinutes,
    WorkplacePreference Workplace,
    ExperienceLevel Experience,
    JobType JobType,
    string TelegramChatId,
    string BotToken
) : IRequest;

public class UpdateSearchProfileCommandHandler : IRequestHandler<UpdateSearchProfileCommand>
{
    private readonly KlarioDbContext _context;

    public UpdateSearchProfileCommandHandler(KlarioDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateSearchProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SearchProfiles
            .FirstOrDefaultAsync(sp => sp.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new EntityNotFoundException(nameof(SearchProfile), request.Id);
        }

        entity.Name = request.Name;
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.TargetJobTitles = request.TargetJobTitles;
        entity.TargetLocations = request.TargetLocations;
        entity.MaxPostingAgeMinutes = request.MaxPostingAgeMinutes;
        entity.Workplace = request.Workplace;
        entity.Experience = request.Experience;
        entity.JobType = request.JobType;
        entity.TelegramChatId = request.TelegramChatId;

        if (!string.IsNullOrWhiteSpace(request.BotToken))
        {
            entity.BotToken = request.BotToken.Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class UpdateSearchProfileCommandValidator : AbstractValidator<UpdateSearchProfileCommand>
{
    public UpdateSearchProfileCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.TargetJobTitles)
            .NotEmpty().WithMessage("At least one target job title is required.")
            .Must(titles => titles != null && titles.Any(t => !string.IsNullOrWhiteSpace(t)))
            .WithMessage("At least one target job title must be non-empty.");

        RuleFor(x => x.TargetLocations)
            .NotEmpty().WithMessage("At least one target location is required.")
            .Must(locs => locs != null && locs.Any(l => !string.IsNullOrWhiteSpace(l)))
            .WithMessage("At least one target location must be non-empty.");

        RuleFor(x => x.MaxPostingAgeMinutes)
            .GreaterThan(0).WithMessage("MaxPostingAgeMinutes must be greater than 0.");

        RuleFor(x => x.Workplace)
            .IsInEnum().WithMessage("Invalid workplace preference value.");

        RuleFor(x => x.Experience)
            .IsInEnum().WithMessage("Invalid experience level value.");

        RuleFor(x => x.JobType)
            .IsInEnum().WithMessage("Invalid job type value.");

        RuleFor(x => x.TelegramChatId)
            .NotEmpty().WithMessage("TelegramChatId is required.")
            .MaximumLength(50).WithMessage("TelegramChatId must not exceed 50 characters.");

        RuleFor(x => x.BotToken)
            .MaximumLength(150).WithMessage("BotToken must not exceed 150 characters.")
            .When(x => !string.IsNullOrEmpty(x.BotToken));
    }
}
