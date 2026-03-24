using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Commands.Auth;

public sealed record RefreshTokenCommand(string Email, string RefreshToken) : IRequest<AuthResult>;
