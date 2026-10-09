using AutoMapper;
using Klario.BLL.Exceptions;
using Klario.BLL.Interfaces;
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
    bool IsActive,
    List<string> TargetJobTitles,
    List<string> TargetLocations,
    TimeSpan MaxPostingAge,
    WorkplacePreference Workplace,
    ExperienceLevel Experience,
    JobType JobType,
    string TelegramChatId,
    string BotToken
) : IRequest;

public class UpdateSearchProfileCommandHandler : IRequestHandler<UpdateSearchProfileCommand>
{
    private readonly KlarioDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPostingScheduleManager _scheduleManager;

    public UpdateSearchProfileCommandHandler(
        KlarioDbContext context,
        IMapper mapper,
        IPostingScheduleManager scheduleManager)
    {
        _context = context;
        _mapper = mapper;
        _scheduleManager = scheduleManager;
    }

    public async Task Handle(UpdateSearchProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SearchProfiles
            .FirstOrDefaultAsync(sp => sp.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new EntityNotFoundException(nameof(SearchProfile), request.Id);
        }

        _mapper.Map(request, entity);
        await _context.SaveChangesAsync(cancellationToken);

        if (entity.IsActive && !entity.IsDeleted)
        {
            _scheduleManager.ScheduleRecurringIngestion(entity.Id);
        }
        else
        {
            _scheduleManager.RemoveRecurringIngestion(entity.Id);
        }
    }
}
