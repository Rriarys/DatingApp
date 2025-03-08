namespace DatingApp.Models
{
    public class ProfilePhoto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
    }
}
