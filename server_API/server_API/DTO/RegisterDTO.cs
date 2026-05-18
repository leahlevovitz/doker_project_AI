namespace server_API.DTO
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterDTO
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string userName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string PasswordHash { get; set; }

        [Required]
        public string adress { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        public string phone { get; set; }
    }


}
