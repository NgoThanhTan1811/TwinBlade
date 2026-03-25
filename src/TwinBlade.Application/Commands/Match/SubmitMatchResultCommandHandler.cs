using MediatR;
using TwinBlade.Application.Abstractions.Caching;
using TwinBlade.Application.Abstractions.Persistence;
using TwinBlade.Application.Dtos.Response;
using TwinBlade.Domain.Entities;
using TwinBlade.Domain.Enums;

namespace TwinBlade.Application.Commands.Match;

public sealed class SubmitMatchResultCommandHandler(
    IMatchResultRepository matchResultRepository,
    IRoomRepository roomRepository,
    IPlayerRepository playerRepository,
    IRoomStateService roomStateService)
    : IRequestHandler<SubmitMatchResultCommand, MatchResultResponse>
{
    public async Task<MatchResultResponse> Handle(SubmitMatchResultCommand request, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetByIdAsync(request.RoomId, cancellationToken)
                   ?? throw new InvalidOperationException("Room not found.");

        if (room.Status != RoomStatus.InGame)
            throw new InvalidOperationException("Room is not in game.");

        // Award gold and merge inventory to each player
        foreach (var entry in request.Players)
        {
            var player = await playerRepository.GetByIdAsync(entry.PlayerId, cancellationToken);
            if (player is null) continue;

            // Award gold
            player.Progress.Gold += entry.EarnedGold;

            // Update boss card   


        }

        var matchResult = new MatchResult
        {
            Id = Guid.NewGuid(),
            RoomId = request.RoomId,
            FinishedAt = DateTime.UtcNow,
            Players = request.Players.Select(p => new PlayerMatchResult
            {
                PlayerId = p.PlayerId,
                Score = p.Score,
                EarnedGold = p.EarnedGold
            }).ToList()
        };

        // Clean up runtime state from Redis
        await roomStateService.DeleteRoomStateAsync(request.RoomId, cancellationToken);

        room.Status = RoomStatus.Finished;

        await matchResultRepository.AddAsync(matchResult, cancellationToken);
        await matchResultRepository.SaveChangesAsync(cancellationToken);
        await playerRepository.SaveChangesAsync(cancellationToken);

        return new MatchResultResponse(
            matchResult.Id,
            matchResult.RoomId,
            matchResult.FinishedAt,
            matchResult.Players.Select(p => new PlayerMatchResultResponse(p.PlayerId, p.Score, p.EarnedGold)).ToList()
        );
    }
}
