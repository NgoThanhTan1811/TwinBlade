using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlayer(Guid id, CancellationToken ct)
    {
        var player = await mediator.Send(new GetPlayerByIdQuery(id), ct);
        return player is null ? NotFound() : Ok(player);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var cognitoId = GetCurrentCognitoId();
        if (string.IsNullOrWhiteSpace(cognitoId))
            return Unauthorized();

        var player = await mediator.Send(new GetPlayerQuery(cognitoId), ct);
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

    [HttpGet("avatars")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableAvatars(CancellationToken ct)
    {
        var avatars = await mediator.Send(new GetAvailableAvatarsQuery(), ct);
        return Ok(avatars);
    }

    [HttpPut("avatar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeAvatar([FromBody] ChangeAvatarRequest request, CancellationToken ct)
    {
        var cognitoId = GetCurrentCognitoId();
        if (string.IsNullOrWhiteSpace(cognitoId)) return Unauthorized();

        var player = await mediator.Send(new GetPlayerQuery(cognitoId), ct);
        if (player is null) return NotFound();

        await mediator.Send(new ChangeAvatarCommand(player.Id, request.AvatarFileName), ct);
        return NoContent();
    }

    private string? GetCurrentCognitoId()
    {
        var cognitoId = User.FindFirst("sub")?.Value;
        if (!string.IsNullOrWhiteSpace(cognitoId)) return cognitoId;

        cognitoId = User.FindFirst("username")?.Value;
        if (!string.IsNullOrWhiteSpace(cognitoId)) return cognitoId;

        cognitoId = User.FindFirst("cognito:username")?.Value;
        if (!string.IsNullOrWhiteSpace(cognitoId)) return cognitoId;

        cognitoId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(cognitoId)) return cognitoId;

        return User.Identity?.Name;
    }
}
