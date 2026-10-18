using AutoMapper;
using FluentValidation;
using Klario.DAL.Context;
using Klario.Common.Enums;
using Klario.DAL.Models;
using MediatR;

namespace Klario.BLL.Commands.SearchProfiles;

public record CreateSearchProfileCommand(
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
) : IRequest<Guid>;

public class CreateSearchProfileCommandHandler : IRequestHandler<CreateSearchProfileCommand, Guid>
{
    private readonly KlarioDbContext _context;
    private readonly IMapper _mapper;

    public CreateSearchProfileCommandHandler(
        KlarioDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Guid> Handle(CreateSearchProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<SearchProfile>(request);
        entity.IsActive = true;

        _context.SearchProfiles.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}

public class CreateSearchProfileCommandValidator : AbstractValidator<CreateSearchProfileCommand>
{
    public CreateSearchProfileCommandValidator()
    {
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
            .NotEmpty().WithMessage("BotToken is required.")
            .MaximumLength(150).WithMessage("BotToken must not exceed 150 characters.");
    }
}
