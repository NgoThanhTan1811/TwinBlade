using MediatR;
using Microsoft.AspNetCore.Mvc;
using TwinBlade.Application.Commands.Auth;
using TwinBlade.Application.Dtos.Request;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
}
