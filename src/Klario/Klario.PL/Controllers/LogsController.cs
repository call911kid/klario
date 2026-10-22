using System;
using System.Threading;
using System.Threading.Tasks;
using Klario.BLL.Queries.AuditLogs;
using Klario.PL.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klario.PL.Controllers;

[Authorize]
[ApiController]
[Route("api/logs")]
public class LogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs(
        [FromQuery] int limit,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var logs = await _mediator.Send(new GetAuditLogsQuery(limit, from, to), cancellationToken);
        return Ok(ApiResponse.Success(logs));
    }
}
