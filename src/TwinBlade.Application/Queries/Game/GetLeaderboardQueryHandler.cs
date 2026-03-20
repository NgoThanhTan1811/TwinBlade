using MediatR;
using TwinBlade.Application.Abstractions.Caching;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Game;

public sealed class GetLeaderboardQueryHandler(ILeaderboardService leaderboardService)
    : IRequestHandler<GetLeaderboardQuery, LeaderboardResponse>
{
    public async Task<LeaderboardResponse> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var topEntries = await leaderboardService.GetTopAsync(request.TopCount, cancellationToken);
        
        int? currentPlayerRank = null;
        if (request.CurrentPlayerId.HasValue)
            currentPlayerRank = await leaderboardService.GetPlayerRankAsync(request.CurrentPlayerId.Value, cancellationToken);

        var entries = topEntries.Select((e, index) => new LeaderboardEntryResponse(
            index + 1,
            e.PlayerId,
            e.DisplayName,
            e.Score,
            e.HighestFloor,
            e.AchievedAt
        )).ToList();

        return new LeaderboardResponse(entries, currentPlayerRank);
    }
}
