using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwinBlade.Application.Commands.Game;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Application.Queries.Game;

namespace TwinBlade.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class GameController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get minimal room session state (Mirror handles gameplay state)
    /// </summary>
    [HttpGet("room/{roomId:guid}/state")]
    [ProducesResponseType(typeof(RoomStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoomState(Guid roomId, CancellationToken ct)
    {
        var state = await mediator.Send(new GetRoomStateQuery(roomId), ct);
        return state is null ? NotFound() : Ok(state);
    }

    /// <summary>
    /// Activate boss map (validates and consumes 3 boss keys from persistent storage)
    /// </summary>
    [HttpPost("room/{roomId:guid}/boss-map/activate")]
    [ProducesResponseType(typeof(RoomStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActivateBossMap(Guid roomId, CancellationToken ct)
    {
        var playerId = GetCurrentPlayerId();
        if (playerId is null) return Unauthorized();

        var result = await mediator.Send(new ActivateBossMapCommand(roomId, playerId.Value), ct);
        return Ok(result);
    }

    private Guid? GetCurrentPlayerId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
