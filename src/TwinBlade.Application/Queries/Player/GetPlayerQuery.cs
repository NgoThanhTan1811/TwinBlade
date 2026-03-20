using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Player;

public sealed record GetPlayerQuery(Guid PlayerId) : IRequest<PlayerResponse?>;
