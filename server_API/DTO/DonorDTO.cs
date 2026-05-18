using api_server.Model;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace server_API.DTO
{
    public class DonorDTO
    {
      
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, ErrorMessage = "Name can be up to 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        public ICollection<GiftDTO> GiftList { get; set; } = new List<GiftDTO>();
    }
}
