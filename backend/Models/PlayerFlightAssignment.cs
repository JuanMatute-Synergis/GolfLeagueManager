using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GolfLeagueManager
{
    public class PlayerFlightAssignment
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid PlayerId { get; set; }
        
        [Required]
        public Guid FlightId { get; set; }
        
        public bool IsFlightLeader { get; set; }
        
        public double HandicapAtAssignment { get; set; }
        
        // Navigation properties - nullable for JSON serialization
        [JsonIgnore]
        public Player? Player { get; set; }
        
        [JsonIgnore]
        public Flight? Flight { get; set; }
    }
}