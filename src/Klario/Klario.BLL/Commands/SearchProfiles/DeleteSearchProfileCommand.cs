using Klario.BLL.Exceptions;
using Klario.DAL.Context;
using Klario.DAL.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Commands.SearchProfiles;

public record DeleteSearchProfileCommand(Guid Id) : IRequest<bool>;

public class DeleteSearchProfileCommandHandler : IRequestHandler<DeleteSearchProfileCommand, bool>
{
    private readonly KlarioDbContext _context;

    public DeleteSearchProfileCommandHandler(KlarioDbContext context)
    {
        _context = context;
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

        return true;
    }
}
