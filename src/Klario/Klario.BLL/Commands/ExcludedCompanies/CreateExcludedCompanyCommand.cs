using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Klario.DAL.Context;
using Klario.DAL.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Commands.ExcludedCompanies;

public record CreateExcludedCompanyCommand(string Name) : IRequest<Guid>;

public class CreateExcludedCompanyCommandHandler : IRequestHandler<CreateExcludedCompanyCommand, Guid>
{
    private readonly KlarioDbContext _context;

    public CreateExcludedCompanyCommandHandler(KlarioDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateExcludedCompanyCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.Name.Trim().ToLowerInvariant();

        // if existing record exists (including soft-deleted)
        var existing = await _context.ExcludedCompanies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Name == normalized, cancellationToken);

        if (existing is not null)
        {
            if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                await _context.SaveChangesAsync(cancellationToken);
            }
            return existing.Id;
        }

        var entity = new ExcludedCompany
        {
            Name = normalized
        };

        _context.ExcludedCompanies.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}

public class CreateExcludedCompanyCommandValidator : AbstractValidator<CreateExcludedCompanyCommand>
{
    public CreateExcludedCompanyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
    }
}
