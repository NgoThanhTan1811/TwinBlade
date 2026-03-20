using MediatR;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Match;

public sealed class GetMatchResultQueryHandler(IMatchResultRepository matchResultRepository)
    : IRequestHandler<GetMatchResultQuery, MatchResultResponse?>
{
    public async Task<MatchResultResponse?> Handle(GetMatchResultQuery request, CancellationToken cancellationToken)
    {
        var result = await matchResultRepository.GetByIdAsync(request.MatchResultId, cancellationToken);
        if (result is null) return null;

        return new MatchResultResponse(
            result.Id,
            result.RoomId,
            result.FinishedAt,
            result.Players.Select(p => new PlayerMatchResultResponse(p.PlayerId, p.Score, p.EarnedGold)).ToList()
        );
    }
}
