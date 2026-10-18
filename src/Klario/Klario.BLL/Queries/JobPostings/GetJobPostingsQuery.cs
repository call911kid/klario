using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Klario.Common.Enums;
using Klario.Common.Pagination;
using Klario.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Queries.JobPostings;

public record GetJobPostingsQuery(
    PagingQuery Paging,
    PostingSource? Source = null,
    string? Search = null
) : IRequest<PagedResponse<GetJobPostingsResponse>>;

public sealed record GetJobPostingsResponse(
    Guid Id,
    string ExternalJobId,
    string Title,
    string Company,
    string Location,
    string Url,
    DateTime? PostedAt,
    PostingSource Source,
    string? DescriptionSnippet,
    DateTime CreatedAt
);

public class GetJobPostingsQueryHandler : IRequestHandler<GetJobPostingsQuery, PagedResponse<GetJobPostingsResponse>>
{
    private readonly KlarioDbContext _context;

    public GetJobPostingsQueryHandler(KlarioDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<GetJobPostingsResponse>> Handle(GetJobPostingsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Paging.Page;
        var pageSize = request.Paging.PageSize;

        var query = _context.JobPostings.AsNoTracking();

        if (request.Source.HasValue)
        {
            query = query.Where(p => p.Source == request.Source.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(p => EF.Functions.Like(p.Title, $"%{term}%") || EF.Functions.Like(p.Company, $"%{term}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new GetJobPostingsResponse(
                p.Id,
                p.ExternalJobId,
                p.Title,
                p.Company,
                p.Location,
                p.Url,
                p.PostedAt,
                p.Source,
                p.DescriptionSnippet,
                p.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResponse<GetJobPostingsResponse>(items, page, pageSize, totalCount);
    }
}

public class GetJobPostingsQueryValidator : AbstractValidator<GetJobPostingsQuery>
{
    public GetJobPostingsQueryValidator()
    {
        RuleFor(x => x.Paging)
            .NotNull().WithMessage("Paging parameters are required.");

        When(x => x.Paging is not null, () =>
        {
            RuleFor(x => x.Paging.Page)
                .GreaterThan(0).WithMessage("Page must be greater than or equal to 1.");

            RuleFor(x => x.Paging.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
        });

        When(x => !string.IsNullOrEmpty(x.Search), () =>
        {
            RuleFor(x => x.Search)
                .MaximumLength(200).WithMessage("Search term must not exceed 200 characters.");
        });
    }
}
