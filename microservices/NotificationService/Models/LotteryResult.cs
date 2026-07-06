using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationService.Models;

public class LotteryResult
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public int GiftId { get; set; }
    public string GiftName { get; set; } = string.Empty;
    public int WinnerUserId { get; set; }
    public string WinnerName { get; set; } = string.Empty;
    public DateTime LotteryDate { get; set; } = DateTime.UtcNow;
}
