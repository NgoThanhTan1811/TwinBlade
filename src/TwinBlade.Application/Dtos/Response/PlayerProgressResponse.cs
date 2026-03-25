namespace TwinBlade.Application.Dtos.Response;

public sealed record PlayerProgressResponse(
    int Gold,
    int HasBossCard,
    DateTime UpdatedAt,
    List<PlayerItemResponse> Items
);

public sealed record PlayerItemResponse(
    Guid ItemId,
    string ItemCode,
    string ItemName,
    int Quantity
);

public sealed record PlayerEquipmentResponse(
    string Slot,
    Guid? ItemId,
    string? ItemCode,
    string? ItemName
);
