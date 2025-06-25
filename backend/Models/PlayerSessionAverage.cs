using System.Text.Json.Serialization;

namespace GolfLeagueManager
{
    /// <summary>
    /// Represents a player's initial average score for a specific session within a season.
    /// This allows for session-specific baseline averages instead of using the global initial average.
    /// </summary>
    public class PlayerSessionAverage
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public Guid SeasonId { get; set; }
        public int SessionStartWeekNumber { get; set; } // The week number where this session starts
        public decimal SessionInitialAverage { get; set; } // The initial average to use for this session
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation properties
        [JsonIgnore]
        public Player Player { get; set; } = null!;
        [JsonIgnore]
        public Season Season { get; set; } = null!;
    }
}
