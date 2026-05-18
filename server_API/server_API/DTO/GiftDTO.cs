using api_server.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace server_API.DTO
{
    public class GiftDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Gift name is required")]
        [StringLength(100, ErrorMessage = "Gift name can be up to 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "DonorId is required")]
        public int DonorId { get; set; }  // שולחים רק את מזהה התורם

        public string? donorName { get; set; } // אופציונלי להצגה בלבד

        [Required(ErrorMessage = "Image URL is required")]
        public string Image { get; set; }

        public string Category { get; set; } = "All_prizes";

        [Range(20, 150, ErrorMessage = "Price must be between 20 and 150")]
        public decimal price { get; set; } = 20m;

        public int Quantity { get; set; }

        public bool IsLocked { get; set; } = false;

        public int PurchasersCount { get; set; }

        //private ICollection<Purchaser> Purchases = new List<Purchaser>();

    }
}
