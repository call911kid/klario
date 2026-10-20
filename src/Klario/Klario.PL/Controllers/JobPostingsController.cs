using System;
using System.Threading;
using System.Threading.Tasks;
using Klario.BLL.Queries.JobPostings;
using Klario.Common.Enums;
using Klario.Common.Pagination;
using Klario.PL.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klario.PL.Controllers;

[Authorize]
[ApiController]
[Route("api/job-postings")]
public class JobPostingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobPostingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PagingQuery paging,
        [FromQuery] PostingSource? source,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var query = new GetJobPostingsQuery(paging, source, search);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(ApiResponse.Success(result));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var stats = await _mediator.Send(new GetJobPostingStatsQuery(), cancellationToken);
        return Ok(ApiResponse.Success(stats));
    }
}
