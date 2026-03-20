using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Match;

public sealed record GetMatchResultQuery(Guid MatchResultId) : IRequest<MatchResultResponse?>;
