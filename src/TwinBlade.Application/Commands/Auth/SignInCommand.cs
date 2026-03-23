using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Commands.Auth;

public sealed record SignInCommand(string Email, string Password) : IRequest<AuthResult>;
