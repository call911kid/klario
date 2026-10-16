using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Klario.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Queries.ExcludedCompanies;

public record GetExcludedCompaniesQuery(
    IReadOnlyList<string>? CompanyNames = null
) : IRequest<List<ExcludedCompanyResponse>>;

public sealed record ExcludedCompanyResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt
);

public class GetExcludedCompaniesQueryHandler : IRequestHandler<GetExcludedCompaniesQuery, List<ExcludedCompanyResponse>>
{
    private readonly KlarioDbContext _context;

    public GetExcludedCompaniesQueryHandler(KlarioDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExcludedCompanyResponse>> Handle(
        GetExcludedCompaniesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.ExcludedCompanies
            .AsNoTracking();

        if (request.CompanyNames is { Count: > 0 })
        {
            var normalizedNames = request.CompanyNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            query = query.Where(c => normalizedNames.Contains(c.Name));
        }
        return await query
            .OrderBy(c => c.Name)
            .Select(c => new ExcludedCompanyResponse(c.Id, c.Name, c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
