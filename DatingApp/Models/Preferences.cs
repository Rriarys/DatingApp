namespace DatingApp.Models
{
    public class Preferences
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PreferredGender { get; set; } = "Any";
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 99;
        public double MaxDistanceKm { get; set; } = 50;

        // максимальные значения по умолчанию
    }
}
