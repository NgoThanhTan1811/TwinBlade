using MediatR;

namespace TwinBlade.Application.Commands.Auth;

public sealed record ResendConfirmationCodeCommand(string Email) : IRequest;
