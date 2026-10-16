using System;
using System.Threading;
using System.Threading.Tasks;
using Klario.BLL.Exceptions;
using Klario.DAL.Context;
using Klario.DAL.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Commands.ExcludedCompanies;

public record DeleteExcludedCompanyCommand(Guid Id) : IRequest<bool>;

public class DeleteExcludedCompanyCommandHandler : IRequestHandler<DeleteExcludedCompanyCommand, bool>
{
    private readonly KlarioDbContext _context;

    public DeleteExcludedCompanyCommandHandler(KlarioDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteExcludedCompanyCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ExcludedCompanies
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new EntityNotFoundException(nameof(ExcludedCompany), request.Id);
        }

        entity.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
