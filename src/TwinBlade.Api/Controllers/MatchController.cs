using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwinBlade.Application.Commands.Match;
using TwinBlade.Application.Dtos.Request;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Application.Queries.Match;

namespace TwinBlade.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MatchController(IMediator mediator) : ControllerBase
{
    [HttpPost("submit")]
    [ProducesResponseType(typeof(MatchResultResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit([FromBody] SubmitMatchResultRequest request, CancellationToken ct)
    {
        var command = new SubmitMatchResultCommand(
            request.RoomId,
            request.Players.Select(p => new PlayerMatchResultEntry(p.PlayerId, p.Score, p.EarnedGold)).ToList()
        );

        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MatchResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResult(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMatchResultQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
