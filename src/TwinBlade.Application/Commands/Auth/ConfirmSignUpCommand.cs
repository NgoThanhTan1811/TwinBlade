using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Commands.Auth;

public sealed record ConfirmSignUpCommand(string Email, string ConfirmationCode) : IRequest;