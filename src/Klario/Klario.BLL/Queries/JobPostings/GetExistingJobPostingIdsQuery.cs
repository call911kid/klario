using Klario.Common.Enums;
using Klario.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Queries.JobPostings;

public record GetExistingJobPostingIdsQuery(
    PostingSource Source,
    IReadOnlyList<string> ExternalIds
) : IRequest<HashSet<string>>;

public class GetExistingJobPostingIdsQueryHandler : IRequestHandler<GetExistingJobPostingIdsQuery, HashSet<string>>
{
    private readonly KlarioDbContext _context;

    public GetExistingJobPostingIdsQueryHandler(KlarioDbContext context)
    {
        _context = context;
    }

    public async Task<HashSet<string>> Handle(
        GetExistingJobPostingIdsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ExternalIds.Count == 0)
            return [];

        var existingIds = await _context.JobPostings
            .AsNoTracking()
            .Where(jp => jp.Source == request.Source && request.ExternalIds.Contains(jp.ExternalJobId))
            .Select(jp => jp.ExternalJobId)
            .ToListAsync(cancellationToken);

        return existingIds.ToHashSet();
    }
}
