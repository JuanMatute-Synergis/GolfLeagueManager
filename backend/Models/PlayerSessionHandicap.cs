using System.Text.Json.Serialization;

namespace GolfLeagueManager
{
    /// <summary>
    /// Represents a player's initial handicap for a specific session within a season.
    /// This allows for session-specific baseline handicaps instead of using the global current handicap.
    /// </summary>
    public class PlayerSessionHandicap
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public Guid SeasonId { get; set; }
        public int SessionStartWeekNumber { get; set; } // The week number where this session starts
        public decimal SessionInitialHandicap { get; set; } // The initial handicap to use for this session
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation properties
        [JsonIgnore]
        public Player Player { get; set; } = null!;
        [JsonIgnore]
        public Season Season { get; set; } = null!;
    }
}
