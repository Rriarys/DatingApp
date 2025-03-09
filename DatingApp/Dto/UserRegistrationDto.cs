using System.ComponentModel.DataAnnotations;

namespace DatingApp.Dto
{
    public class UserRegistrationDto
    {
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string PasswordHash { get; set; }
    }
}
