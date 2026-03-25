namespace TwinBlade.Application.Dtos.Response;

public sealed record ItemResponse(
    Guid Id,
    string Code,      // Format: {type}_{material} (e.g., "kiem_go")
    string Type,      // Extracted type code (e.g., "kiem")
    string Material,  // Extracted material code (e.g., "go")
    string Name,
    string Description,
    string ImageUrl
);
