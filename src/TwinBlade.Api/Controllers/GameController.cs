using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwinBlade.Application.Commands.Game;
using TwinBlade.Application.Dtos.Request;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Application.Queries.Game;

namespace TwinBlade.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class GameController(IMediator mediator) : ControllerBase
{
    [HttpGet("room/{roomId:guid}/state")]
    [ProducesResponseType(typeof(RoomStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoomState(Guid roomId, CancellationToken ct)
    {
        var state = await mediator.Send(new GetRoomStateQuery(roomId), ct);
        return state is null ? NotFound() : Ok(state);
    }

    [HttpPost("room/{roomId:guid}/damage")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> TakeDamage(Guid roomId, [FromBody] UpdatePlayerHpRequest request, CancellationToken ct)
    {
        var playerId = GetCurrentPlayerId();
        if (playerId is null) return Unauthorized();

        await mediator.Send(new UpdatePlayerHpCommand(roomId, playerId.Value, request.Damage), ct);
        return NoContent();
    }

    [HttpPost("room/{roomId:guid}/revive-card")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PickReviveCard(Guid roomId, CancellationToken ct)
    {
        var playerId = GetCurrentPlayerId();
        if (playerId is null) return Unauthorized();

        await mediator.Send(new PickReviveCardCommand(roomId, playerId.Value), ct);
        return NoContent();
    }

    [HttpPost("room/{roomId:guid}/revive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevivePlayer(Guid roomId, [FromBody] RevivePlayerRequest request, CancellationToken ct)
    {
        var playerId = GetCurrentPlayerId();
        if (playerId is null) return Unauthorized();

        await mediator.Send(new RevivePlayerCommand(roomId, playerId.Value, request.TargetPlayerId), ct);
        return NoContent();
    }

    [HttpGet("leaderboard")]
    [ProducesResponseType(typeof(LeaderboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaderboard([FromQuery] int top = 10, CancellationToken ct = default)
    {
        var playerId = GetCurrentPlayerId();
        var result = await mediator.Send(new GetLeaderboardQuery(playerId, top), ct);
        return Ok(result);
    }

    private Guid? GetCurrentPlayerId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
