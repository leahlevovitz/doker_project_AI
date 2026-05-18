using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace server_API.Model
{
    public class User
    {
        [JsonIgnore]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, ErrorMessage = "Name can be up to 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(30, ErrorMessage = "Username can be up to 30 characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string adress { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        public string phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; }
    }
}
