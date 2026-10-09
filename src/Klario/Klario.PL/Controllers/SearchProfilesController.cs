using AutoMapper;
using Klario.BLL.Commands.SearchProfiles;
using Klario.BLL.Queries.SearchProfiles;
using Klario.PL.DTOs.Requests;
using Klario.PL.DTOs.Responses;
using Klario.PL.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Klario.PL.Controllers;

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
    public async Task<IActionResult> Create([FromBody] CreateSearchProfileRequestDto dto)
    {
        var command = _mapper.Map<CreateSearchProfileCommand>(dto);
        var id = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Success(id));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllSearchProfilesQuery();
        var responses = await _mediator.Send(query);
        var responseDtos = _mapper.Map<List<SearchProfileResponseDto>>(responses);

        return Ok(ApiResponse.Success(responseDtos));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetSearchProfileByIdQuery(id);
        var response = await _mediator.Send(query);
        var responseDto = _mapper.Map<SearchProfileResponseDto>(response);

        return Ok(ApiResponse.Success(responseDto));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSearchProfileRequestDto dto)
    {
        var command = new UpdateSearchProfileCommand(
            id,
            dto.Name,
            dto.Description,
            dto.IsActive,
            dto.TargetJobTitles,
            dto.TargetLocations,
            dto.MaxPostingAgeMinutes,
            dto.IntervalMinutes,
            dto.Workplace,
            dto.Experience,
            dto.JobType,
            dto.TelegramChatId,
            dto.BotToken
        );

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
