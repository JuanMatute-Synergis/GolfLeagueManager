using System.Text.Json.Serialization;

namespace GolfLeagueManager
{
    public class PlayerFlightAssignment
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public Guid FlightId { get; set; }
        public bool IsFlightLeader { get; set; }
        public double HandicapAtAssignment { get; set; }
        
        // Navigation properties
        [JsonIgnore]
        public Player Player { get; set; } = null!;
        [JsonIgnore]
        public Flight Flight { get; set; } = null!;
    }
}