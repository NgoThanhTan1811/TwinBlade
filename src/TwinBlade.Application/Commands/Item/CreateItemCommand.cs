using MediatR;
using TwinBlade.Application.Dtos.Response;

namespace TwinBlade.Application.Commands.Item;

public sealed record CreateItemCommand(
    string Code, // Format: {type}_{material} (e.g., "kiem_go")
    string Name,
    string? Description = null // Optional custom description
) : IRequest<ItemResponse>;
