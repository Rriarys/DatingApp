using System.ComponentModel.DataAnnotations;

namespace DatingApp.Models
{
    public class AboutUser
    {
        public int Id { get; set; } // pk
        public int UserId { get; set; } // fk
        public string FirstName { get; set; } = string.Empty; // имя не требуем
        public DateTime BirthDate { get; set; }
        public string About { get; set; } = string.Empty; // не требуем 
    }
}
