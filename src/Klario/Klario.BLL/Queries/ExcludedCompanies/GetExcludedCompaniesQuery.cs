using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Klario.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Queries.ExcludedCompanies;

public record GetExcludedCompaniesQuery : IRequest<List<ExcludedCompanyResponse>>;

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
        return await _context.ExcludedCompanies
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new ExcludedCompanyResponse(c.Id, c.Name, c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
