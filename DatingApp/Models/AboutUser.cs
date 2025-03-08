using System.ComponentModel.DataAnnotations;

namespace DatingApp.Models
{
    public class AboutUser
    {
        public int Id { get; set; } // pk
        public int UserId { get; set; } // fk
        [Required]
        public required string FirstName { get; set; } // требуем при регистрации
        [Required]
        public required DateTime BirthDate { get; set; } // требуем при регистрации
        public string About { get; set; } = string.Empty; // не требуем вообще
    }
}
