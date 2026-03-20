using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Game;

public sealed record GetLeaderboardQuery(Guid? CurrentPlayerId = null, int TopCount = 10) : IRequest<LeaderboardResponse>;
