namespace DatingApp.Dto
{
    public class UserProfileDto
    {
        public string FirstName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string About { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PreferredGender { get; set; } = "Any";
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 99;
        public int MaxDistanceKm { get; set; } = 50;
    }
}
