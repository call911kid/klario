using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Klario.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Queries.ExcludedCompanies;

public record GetMatchingExcludedCompaniesQuery(
    IReadOnlyList<string> CompanyNames
) : IRequest<HashSet<string>>;

public class GetMatchingExcludedCompaniesQueryHandler : IRequestHandler<GetMatchingExcludedCompaniesQuery, HashSet<string>>
{
    private readonly KlarioDbContext _context;

    public GetMatchingExcludedCompaniesQueryHandler(KlarioDbContext context)
    {
        _context = context;
    }

    public async Task<HashSet<string>> Handle(
        GetMatchingExcludedCompaniesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CompanyNames is not { Count: > 0 })
        {
            return [];
        }

        var normalizedNames = request.CompanyNames
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        if (normalizedNames.Count == 0)
        {
            return [];
        }

        var matchedNormalized = await _context.ExcludedCompanies
            .AsNoTracking()
            .Where(c => normalizedNames.Contains(c.NormalizedName))
            .Select(c => c.NormalizedName)
            .ToListAsync(cancellationToken);

        return matchedNormalized.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
