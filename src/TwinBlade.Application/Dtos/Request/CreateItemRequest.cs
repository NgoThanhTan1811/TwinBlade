namespace TwinBlade.Application.Dtos.Request;

public sealed record CreateItemRequest(
    string Code, // Format: {type}_{material} (e.g., "kiem_go" = sword_wood)
    string Name,
    string? Description = null // Optional custom description
);
