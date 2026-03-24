using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Player;

public sealed record GetPlayerByIdQuery(Guid PlayerId) : IRequest<PlayerResponse?>;
