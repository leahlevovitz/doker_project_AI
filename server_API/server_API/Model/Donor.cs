using api_server.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api_server.Models
{
    [Table("Donors")]
    public class Donor
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public ICollection<Gift> GiftList { get; set; } = new List<Gift>();
    }
}
