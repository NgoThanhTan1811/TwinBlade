using MediatR;
using TwinBlade.Application.Abstractions.Auth;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Commands.Auth;

public sealed class RefreshTokenCommandHandler(ICognitoAuthService cognitoAuthService)
    : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    public Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        => cognitoAuthService.RefreshTokenAsync(request.Email, request.RefreshToken, cancellationToken);
}
