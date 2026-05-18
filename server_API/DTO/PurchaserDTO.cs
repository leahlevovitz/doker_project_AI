using System.Text.Json.Serialization;

namespace server_API.DTO
{
    public class PurchaserDTO
    {
   
        public int Id { get; set; }
        public int GiftId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; } 
        public string ?Name { get; set; }
        public string ?UserPhone { get; set; }
        public string ?UserEmail { get; set; }
        public string ?UserAddress { get; set; }
        public string GiftName { get; set; }
        public decimal GiftPrice { get; set; }
        public string GiftImage { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}
