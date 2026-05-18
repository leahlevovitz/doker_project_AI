using api_server.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api_server.Model
{
    [Table("Gifts")]
    public class Gift
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Name { get; set; }
        public int DonorId { get; set; }  // מפתח זר בלבד
        [ForeignKey("DonorId")]
        [JsonIgnore]
        public Donor Donor { get; set; }
        public int Quantity { get; set; }

        public bool IsLocked { get; set; } = false;
        public ICollection<Lotteries> Lotteries { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Category category { get; set; }

        public decimal price { get; set; } = 20m;



        public string Image { get; set; }  
        public List<Purchaser> Purchases { get; set; } = new List<Purchaser>();

        [NotMapped]
        public int PurchasersCount => Purchases?.Count ?? 0;

    }
   public enum Category
    {
      
     All_prizes=0,

    Vehicles=1,

    Home_and_Family=2,

    Gifts_for_Women=3,

    Gifts_for_Men=4,

    Tourism_and_Vacations=5,

    Kids_Shopping=6,

    Beauty_and_Personal_Care=7,

    Electrical_Appliances=8,

    }
}