using AutoMapper;
using Klario.DAL.Context;
using Klario.DAL.Models;
using Klario.Providers.Contracts;
using MediatR;

namespace Klario.BLL.Commands.JobPostings;

public record CreateJobPostingsCommand(
    IReadOnlyList<ExternalPosting> Postings
) : IRequest;

public class CreateJobPostingsCommandHandler : IRequestHandler<CreateJobPostingsCommand>
{
    private readonly KlarioDbContext _context;
    private readonly IMapper _mapper;

    public CreateJobPostingsCommandHandler(KlarioDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task Handle(
        CreateJobPostingsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Postings.Count == 0)
            return;

        var entities = _mapper.Map<List<JobPosting>>(request.Postings);

        _context.JobPostings.AddRange(entities);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
