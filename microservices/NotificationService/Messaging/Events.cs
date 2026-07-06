namespace Shared.Events;

public record OrderPlacedEvent(
    Guid MessageId,
    int PurchaseId,
    int GiftId,
    int UserId,
    DateTime PlacedAt);

public record InventoryReservedEvent(
    Guid MessageId,
    int PurchaseId,
    int GiftId,
    int UserId);

public record InventoryRejectedEvent(
    Guid MessageId,
    int PurchaseId,
    int GiftId,
    int UserId,
    string Reason);

public record OrderConfirmedEvent(
    Guid MessageId,
    int PurchaseId,
    int GiftId,
    int UserId);

public record OrderCancelledEvent(
    Guid MessageId,
    int PurchaseId,
    int GiftId,
    int UserId,
    string Reason);
