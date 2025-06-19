using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GolfLeagueManager
{
    public class ScoreEntry
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid PlayerId { get; set; }
        
        [Required]
        public Guid WeekId { get; set; }
        
        public int? Score { get; set; } // Nullable to allow absent players (who earn points but have no score)
        
        public int PointsEarned { get; set; }
        
        // Navigation properties - nullable for JSON serialization
        [JsonIgnore]
        public Player? Player { get; set; }
        
        [JsonIgnore]
        public Week? Week { get; set; }
    }
}