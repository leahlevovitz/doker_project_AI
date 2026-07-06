using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProductCatalogService.Models;

public class Gift
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public int DonorId { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public Category Category { get; set; }
    public decimal Price { get; set; } = 20m;
    public string Image { get; set; } = string.Empty;
}

public enum Category
{
    All_prizes = 0,
    Vehicles = 1,
    Home_and_Family = 2,
    Gifts_for_Women = 3,
    Gifts_for_Men = 4,
    Tourism_and_Vacations = 5,
    Kids_Shopping = 6,
    Beauty_and_Personal_Care = 7,
    Electrical_Appliances = 8
}
