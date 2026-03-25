using MediatR;
using TwinBlade.Application.Abstractions.Auth;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Commands.Auth;

public sealed class ConfirmSignUpCommandHandler(ICognitoAuthService cognitoAuthService)
    : IRequestHandler<ConfirmSignUpCommand>
{
    public Task Handle(ConfirmSignUpCommand request, CancellationToken cancellationToken)
        => cognitoAuthService.ConfirmSignUpAsync(request.Email, request.ConfirmationCode, cancellationToken);
}
