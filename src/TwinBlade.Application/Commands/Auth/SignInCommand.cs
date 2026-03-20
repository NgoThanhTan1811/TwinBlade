using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Commands.Auth;

public sealed record SignInCommand(string Username, string Password) : IRequest<AuthResult>;
