using MediatR;
using TwinBlade.Application.Abstractions.Auth;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Commands.Auth;

public sealed class SignInCommandHandler(ICognitoAuthService cognitoAuthService)
    : IRequestHandler<SignInCommand, AuthResult>
{
    public Task<AuthResult> Handle(SignInCommand request, CancellationToken cancellationToken)
        => cognitoAuthService.SignInAsync(request.Username, request.Password, cancellationToken);
}
