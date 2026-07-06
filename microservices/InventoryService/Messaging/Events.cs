namespace Shared.Events;

public record OrderPlacedEvent(
    Guid MessageId,
    Guid CorrelationId,
    int PurchaseId,
    int GiftId,
    int UserId,
    DateTime PlacedAt);

public record InventoryReservedEvent(
    Guid MessageId,
    Guid CorrelationId,
    int PurchaseId,
    int GiftId,
    int UserId);

public record InventoryRejectedEvent(
    Guid MessageId,
    Guid CorrelationId,
    int PurchaseId,
    int GiftId,
    int UserId,
    string Reason);

public record OrderConfirmedEvent(
    Guid MessageId,
    Guid CorrelationId,
    int PurchaseId,
    int GiftId,
    int UserId);

public record OrderCancelledEvent(
    Guid MessageId,
    Guid CorrelationId,
    int PurchaseId,
    int GiftId,
    int UserId,
    string Reason);
