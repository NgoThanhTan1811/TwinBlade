using MediatR;
using TwinBlade.Application.Abstractions.Auth;

namespace TwinBlade.Application.Commands.Auth
{
    public sealed class ResendConfirmationCodeCommandHandler(ICognitoAuthService cognitoAuthService)
        : IRequestHandler<ResendConfirmationCodeCommand>
    {
        public Task Handle(ResendConfirmationCodeCommand request, CancellationToken cancellationToken)
            => cognitoAuthService.ResendConfirmationCodeAsync(request.Email, cancellationToken);
    }
}