using MediatR;
using TwinBlade.Application.Abstractions.Caching;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Game;

public sealed class GetRoomStateQueryHandler(IRoomStateService roomStateService)
    : IRequestHandler<GetRoomStateQuery, RoomStateResponse?>
{
    public async Task<RoomStateResponse?> Handle(GetRoomStateQuery request, CancellationToken cancellationToken)
    {
        var state = await roomStateService.GetRoomStateAsync(request.RoomId, cancellationToken);
        if (state is null) return null;

        return new RoomStateResponse(
            state.RoomId,
            state.RoomCode,
            state.Status,
            state.BossMapActivated,
            state.BossDefeated,
            state.Players.Select(p => new PlayerStateResponse(
                p.PlayerId,
                p.DisplayName
            )).ToList(),
            state.GameStartedAt,
            state.LastActivityAt
        );
    }
}
