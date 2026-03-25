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
    [ProducesResponseType(typeof(SignUpResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SignUp([FromBody] RegisterPlayerRequest request, CancellationToken ct)
    {
        var player = await mediator.Send(
            new RegisterPlayerCommand(request.Email, request.Password, request.Username),
            ct);

        return Ok(new SignUpResponse
        {
            PlayerId = player.Id,
            Email = player.Email,
            RequiresEmailVerification = true,
            Message = "Registration successful. Please verify the code sent to your email."
        });
    }

    [HttpPost("confirm-sign-up")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmSignUp([FromBody] ConfirmSignUpRequest request, CancellationToken ct)
    {
        await mediator.Send(new ConfirmSignUpCommand(request.Email, request.Code), ct);
        return Ok();
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

    [HttpGet("debug/me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult DebugMe()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var sub = User.FindFirst("sub")?.Value;
        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated,
            CognitoId = sub,
            AllClaims = claims
        });
    }
}
