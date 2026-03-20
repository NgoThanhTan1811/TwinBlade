using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwinBlade.Application.Commands.Room;
using TwinBlade.Application.Dtos.Request;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Application.Queries.Room;

namespace TwinBlade.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class RoomController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request, CancellationToken ct)
    {
        var playerId = GetCurrentPlayerId();
        if (playerId is null) return Unauthorized();

        var room = await mediator.Send(new CreateRoomCommand(playerId.Value, request.MaxPlayers), ct);
        return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
    }

    [HttpPost("join")]
    [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> JoinRoom([FromBody] JoinRoomRequest request, CancellationToken ct)
    {
        var playerId = GetCurrentPlayerId();
        if (playerId is null) return Unauthorized();

        var displayName = User.FindFirst("cognito:username")?.Value ?? "Player";
        var room = await mediator.Send(new JoinRoomCommand(request.RoomCode, playerId.Value, displayName), ct);
        return Ok(room);
    }

    [HttpPut("{id:guid}/ready")]
    [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetReady(Guid id, [FromQuery] bool isReady, CancellationToken ct)
    {
        var playerId = GetCurrentPlayerId();
        if (playerId is null) return Unauthorized();

        var room = await mediator.Send(new SetPlayerReadyCommand(id, playerId.Value, isReady), ct);
        return Ok(room);
    }

    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> StartGame(Guid id, CancellationToken ct)
    {
        var playerId = GetCurrentPlayerId();
        if (playerId is null) return Unauthorized();

        var room = await mediator.Send(new StartGameCommand(id, playerId.Value), ct);
        return Ok(room);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoom(Guid id, CancellationToken ct)
    {
        var room = await mediator.Send(new GetRoomQuery(id), ct);
        return room is null ? NotFound() : Ok(room);
    }

    private Guid? GetCurrentPlayerId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
