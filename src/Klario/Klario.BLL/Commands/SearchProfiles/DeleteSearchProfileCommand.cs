using Klario.BLL.Exceptions;
using Klario.BLL.Interfaces;
using Klario.DAL.Context;
using Klario.DAL.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Commands.SearchProfiles;

public record DeleteSearchProfileCommand(Guid Id) : IRequest<bool>;

public class DeleteSearchProfileCommandHandler : IRequestHandler<DeleteSearchProfileCommand, bool>
{
    private readonly KlarioDbContext _context;
    private readonly IPostingScheduleManager _scheduleManager;

    public DeleteSearchProfileCommandHandler(
        KlarioDbContext context,
        IPostingScheduleManager scheduleManager)
    {
        _context = context;
        _scheduleManager = scheduleManager;
    }

    public async Task<bool> Handle(DeleteSearchProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SearchProfiles
            .FirstOrDefaultAsync(sp => sp.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new EntityNotFoundException(nameof(SearchProfile), request.Id);
        }

        entity.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        _scheduleManager.RemoveRecurringIngestion(request.Id);

        return true;
    }
}
