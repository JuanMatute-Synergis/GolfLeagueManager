using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GolfLeagueManager
{
    public class PlayerSeasonRecord
    {
        public Guid Id { get; set; }

        [Required]
        public Guid PlayerId { get; set; }

        [Required]
        public Guid SeasonId { get; set; }

        // Initial values set when player is assigned to flights for this season
        public decimal InitialHandicap { get; set; } = 0;
        public decimal InitialAverageScore { get; set; } = 0;

        // Current calculated values for this season
        public decimal CurrentHandicap { get; set; } = 0;
        public decimal CurrentAverageScore { get; set; } = 0;

        // Tracking information
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [JsonIgnore]
        public Player? Player { get; set; }

        [JsonIgnore]
        public Season? Season { get; set; }
    }
}
