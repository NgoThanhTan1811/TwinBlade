using MediatR;
using Microsoft.AspNetCore.Mvc;
using TwinBlade.Application.Commands.Player;
using TwinBlade.Application.Commands.Auth;
using TwinBlade.Application.Dtos.Request;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("sign-in")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SignIn(
        [FromBody] SignInRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(new SignInCommand(request.Email, request.Password), ct);
        return Ok(result);
    }

    [HttpPost("sign-up")]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> SignUp([FromBody] RegisterPlayerRequest request, CancellationToken ct)
    {
        var player = await mediator.Send(new RegisterPlayerCommand(request.Email, request.Password, request.Username), ct);
        return Created($"auth/sign-up/{player.Id}", player);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(new RefreshTokenCommand(request.Email, request.RefreshToken), ct);
        return Ok(result);
    }
}
