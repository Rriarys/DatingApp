namespace DatingApp.Models
{
    public class UserLocation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public double Latitude { get; set; } // широта
        public double Longitude { get; set; } // долгота

        // будем спрашивать местоположение юзера и ставить гео
    }
}
