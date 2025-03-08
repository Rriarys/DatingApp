using System.ComponentModel.DataAnnotations;

namespace DatingApp.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string PasswordHash { get; set; }

        public AboutUser AboutUser { get; set; } = new();
        public ProfilePhoto ProfilePhoto { get; set; } = new();
        public UserLocation UserLocation { get; set; } = new();
        public Preferences Preferences { get; set; } = new();
        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
