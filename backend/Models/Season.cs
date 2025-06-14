using System.Text.Json.Serialization;

namespace GolfLeagueManager
{
    public class Season
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g. "Spring 2025"
        public int Year { get; set; }
        public int SeasonNumber { get; set; } // e.g. 1, 2, 3
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        [JsonIgnore]
        public List<Week> Weeks { get; set; } = new List<Week>();
        [JsonIgnore]
        public List<Flight> Flights { get; set; } = new List<Flight>();
    }
}
