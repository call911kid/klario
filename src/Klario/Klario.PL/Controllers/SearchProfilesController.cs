using AutoMapper;
using Klario.BLL.Commands.SearchProfiles;
using Klario.BLL.Queries.SearchProfiles;
using Klario.PL.Contracts.SearchProfiles;
using Klario.PL.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klario.PL.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SearchProfilesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SearchProfilesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSearchProfileRequest request)
    {
        var command = new CreateSearchProfileCommand(
            request.Name,
            request.Description,
            request.TargetJobTitles,
            request.TargetLocations,
            request.MaxPostingAgeMinutes,
            request.Workplace,
            request.Experience,
            request.JobType,
            request.TelegramChatId,
            request.BotToken
        );

        var id = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Success(id));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllSearchProfilesQuery();
        var responses = await _mediator.Send(query);
        var result = _mapper.Map<List<SearchProfileResponse>>(responses);

        return Ok(ApiResponse.Success(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetSearchProfileByIdQuery(id);
        var response = await _mediator.Send(query);
        var result = _mapper.Map<SearchProfileResponse>(response);

        return Ok(ApiResponse.Success(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSearchProfileRequest request)
    {
        var command = new UpdateSearchProfileCommand(
            id,
            request.Name,
            request.Description,
            request.TargetJobTitles,
            request.TargetLocations,
            request.MaxPostingAgeMinutes,
            request.Workplace,
            request.Experience,
            request.JobType,
            request.TelegramChatId,
            request.BotToken
        );

        await _mediator.Send(command);

        return Ok(ApiResponse.Success());
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] SetSearchProfileStatusRequest request)
    {
        var command = new SetSearchProfileStatusCommand(id, request.IsActive);
        await _mediator.Send(command);

        return Ok(ApiResponse.Success());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteSearchProfileCommand(id);
        await _mediator.Send(command);

        return Ok(ApiResponse.Success());
    }
}
