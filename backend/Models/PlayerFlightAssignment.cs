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

        [Required]
        public Guid SeasonId { get; set; }

        [Required]
        public int SessionStartWeekNumber { get; set; } // The week number where this session starts

        public bool IsFlightLeader { get; set; }

        // DEPRECATED: Will be moved to PlayerSeasonRecord
        public double HandicapAtAssignment { get; set; }

        public DateTime AssignmentDate { get; set; } = DateTime.UtcNow;

        // Navigation properties - nullable for JSON serialization
        [JsonIgnore]
        public Player? Player { get; set; }

        [JsonIgnore]
        public Flight? Flight { get; set; }

        [JsonIgnore]
        public Season? Season { get; set; }
    }
}