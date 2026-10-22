using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Klario.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Queries.AuditLogs;

public record GetAuditLogsQuery(
    int Limit,
    DateTime? From,
    DateTime? To
) : IRequest<List<AuditLogResponse>>;

public sealed record AuditLogResponse(
    int Id,
    string Message,
    string Level,
    DateTime TimeStamp,
    string? Exception,
    string? MachineName
);

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, List<AuditLogResponse>>
{
    private readonly KlarioDbContext _context;

    public GetAuditLogsQueryHandler(KlarioDbContext context)
    {
        _context = context;
    }

    public async Task<List<AuditLogResponse>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Logs.AsNoTracking();

        if (request.From.HasValue)
        {
            query = query.Where(l => l.TimeStamp >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(l => l.TimeStamp <= request.To.Value);
        }

        return await query
            .OrderByDescending(l => l.TimeStamp)
            .Take(request.Limit)
            .Select(l => new AuditLogResponse(
                l.Id,
                l.Message,
                l.Level,
                l.TimeStamp,
                l.Exception,
                l.MachineName
            ))
            .ToListAsync(cancellationToken);
    }
}

public class GetAuditLogsQueryValidator : AbstractValidator<GetAuditLogsQuery>
{
    public GetAuditLogsQueryValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0.");

        When(x => x.From.HasValue && x.To.HasValue, () =>
        {
            RuleFor(x => x.To)
                .GreaterThanOrEqualTo(x => x.From)
                .WithMessage("'To' timestamp must be greater than or equal to 'From' timestamp.");
        });
    }
}
