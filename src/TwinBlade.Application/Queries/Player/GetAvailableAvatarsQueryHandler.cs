using MediatR;
using TwinBlade.Application.Abstractions.Storage;

namespace TwinBlade.Application.Queries.Player;

public sealed class GetAvailableAvatarsQueryHandler(IAvatarService avatarService)
    : IRequestHandler<GetAvailableAvatarsQuery, List<string>>
{
    public Task<List<string>> Handle(GetAvailableAvatarsQuery request, CancellationToken cancellationToken)
        => avatarService.GetAvailableAvatarsAsync(cancellationToken);
}
