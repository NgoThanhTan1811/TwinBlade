using MediatR;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Domain.Entities;

namespace TwinBlade.Application.Commands.Match;

public sealed record SubmitMatchResultCommand(
    Guid RoomId,
    List<PlayerMatchResultEntry> Players
) : IRequest<MatchResultResponse>;

public sealed record PlayerMatchResultEntry(
    Guid PlayerId,
    int Score,
    int EarnedGold,
    List<RuntimePlayerItem> PickedItems
);
