using api_server.Model;
using server_API.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api_server.Models
{
    public class Purchaser
    {
        [JsonIgnore]
        public int Id { get; set; }
        public int GiftId { get; set; }
        [ForeignKey("GiftId")]
        public Gift Gift { get; set; }
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }


        public DateTime PurchaseDate { get; set; }
        public bool IsDraft { get; set; } = true; // רכישה טיוטא עד אישור
    }
}
