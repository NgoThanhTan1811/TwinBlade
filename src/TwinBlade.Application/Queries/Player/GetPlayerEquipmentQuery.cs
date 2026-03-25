using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Queries.Player;

public sealed record GetPlayerEquipmentQuery(Guid PlayerId) : IRequest<List<PlayerEquipmentResponse>?>;
