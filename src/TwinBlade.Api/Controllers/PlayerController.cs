using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwinBlade.Application.Commands.Player;
using TwinBlade.Application.Dtos.Request;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Application.Queries.Player;

namespace TwinBlade.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class PlayerController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterPlayerRequest request, CancellationToken ct)
    {
        var player = await mediator.Send(new RegisterPlayerCommand(request.Username, request.DisplayName), ct);
        return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlayer(Guid id, CancellationToken ct)
    {
        var player = await mediator.Send(new GetPlayerQuery(id), ct);
        return player is null ? NotFound() : Ok(player);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var sub = User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(sub, out var playerId)) return Unauthorized();

        var player = await mediator.Send(new GetPlayerQuery(playerId), ct);
        return player is null ? NotFound() : Ok(player);
    }

    [HttpGet("{id:guid}/progress")]
    [ProducesResponseType(typeof(PlayerProgressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProgress(Guid id, CancellationToken ct)
    {
        var progress = await mediator.Send(new GetPlayerProgressQuery(id), ct);
        return progress is null ? NotFound() : Ok(progress);
    }
}
