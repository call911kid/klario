using AutoMapper;
using FluentValidation;
using Klario.DAL.Context;
using Klario.DAL.Extensions;
using Klario.DAL.Models;
using Klario.Providers.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Klario.BLL.Commands.JobPostings;

public record CreateJobPostingsCommand(
    IReadOnlyList<ExternalPosting> Postings
) : IRequest<IReadOnlyList<ExternalPosting>>;

public class CreateJobPostingsCommandHandler : IRequestHandler<CreateJobPostingsCommand, IReadOnlyList<ExternalPosting>>
{
    private readonly KlarioDbContext _context;
    private readonly IMapper _mapper;

    public CreateJobPostingsCommandHandler(KlarioDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ExternalPosting>> Handle(
        CreateJobPostingsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Postings.Count == 0)
            return [];

        var pending = _mapper.Map<List<JobPosting>>(request.Postings);
        var inserted = new List<JobPosting>();

        while (pending.Count > 0)
        {
            try
            {
                _context.JobPostings.AddRange(pending);
                await _context.SaveChangesAsync(cancellationToken);
                inserted.AddRange(pending);
                break;
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                _context.ChangeTracker.Clear();

                var pendingIds = pending.Select(p => p.ExternalJobId).ToList();
                var source = pending[0].Source;

                var committedIds = await _context.JobPostings
                    .AsNoTracking()
                    .Where(jp => jp.Source == source && pendingIds.Contains(jp.ExternalJobId))
                    .Select(jp => jp.ExternalJobId)
                    .ToListAsync(cancellationToken);

                pending = pending
                    .Where(p => !committedIds.Contains(p.ExternalJobId))
                    .ToList();
            }
        }

        if (inserted.Count == 0)
            return [];

        var insertedIds = inserted.Select(i => i.ExternalJobId).ToHashSet();
        return request.Postings.Where(p => insertedIds.Contains(p.ExternalId)).ToList();
    }
}

public class CreateJobPostingsCommandValidator : AbstractValidator<CreateJobPostingsCommand>
{
    public CreateJobPostingsCommandValidator()
    {
        RuleFor(x => x.Postings)
            .NotNull().WithMessage("Postings collection cannot be null.");
    }
}
