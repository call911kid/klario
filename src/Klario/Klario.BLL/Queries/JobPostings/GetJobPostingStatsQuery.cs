using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Klario.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Queries.JobPostings;

public record GetJobPostingStatsQuery : IRequest<JobPostingStatsResponse>;

public sealed record JobPostingStatsResponse(int TodayCount, int WeekCount, int TotalCount);

public class GetJobPostingStatsQueryHandler : IRequestHandler<GetJobPostingStatsQuery, JobPostingStatsResponse>
{
    private readonly KlarioDbContext _context;

    public GetJobPostingStatsQueryHandler(KlarioDbContext context)
    {
        _context = context;
    }

    public async Task<JobPostingStatsResponse> Handle(GetJobPostingStatsQuery request, CancellationToken cancellationToken)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var weekStartUtc = todayUtc.AddDays(-(int)todayUtc.DayOfWeek);

        var stats = await _context.JobPostings
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new JobPostingStatsResponse(
                g.Count(j => j.CreatedAt >= todayUtc),
                g.Count(j => j.CreatedAt >= weekStartUtc),
                g.Count()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return stats ?? new JobPostingStatsResponse(0, 0, 0);
    }
}
