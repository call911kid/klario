using Klario.BLL.Commands.Schedules;
using Klario.Common.Enums;
using Klario.PL.Contracts.Schedules;
using Klario.PL.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klario.PL.Controllers;

[Authorize]
[ApiController]
[Route("api/search-profiles/{searchProfileId:guid}/schedules")]
public class SchedulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SchedulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("{source}")]
    public async Task<IActionResult> Configure(
        Guid searchProfileId,
        PostingSource source,
        [FromBody] ConfigureScheduleRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ConfigureScheduleCommand(searchProfileId, source, request.IntervalMinutes),
            cancellationToken);

        return Ok(ApiResponse.Success());
    }

    [HttpPut("{source}/status")]
    public async Task<IActionResult> SetStatus(
        Guid searchProfileId,
        PostingSource source,
        [FromBody] SetScheduleStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new SetScheduleStatusCommand(searchProfileId, source, request.IsEnabled),
            cancellationToken);

        return Ok(ApiResponse.Success());
    }

    [HttpDelete("{source}")]
    public async Task<IActionResult> Remove(
        Guid searchProfileId,
        PostingSource source,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveScheduleCommand(searchProfileId, source), cancellationToken);

        return Ok(ApiResponse.Success());
    }
}
