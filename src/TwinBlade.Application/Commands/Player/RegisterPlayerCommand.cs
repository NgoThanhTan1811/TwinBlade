using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Commands.Player;

public sealed record RegisterPlayerCommand(string Email, string Password, string Username) : IRequest<PlayerResponse>;
