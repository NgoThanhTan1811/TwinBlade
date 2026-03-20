using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Commands.Player;

public sealed record RegisterPlayerCommand(string Username, string DisplayName) : IRequest<PlayerResponse>;
