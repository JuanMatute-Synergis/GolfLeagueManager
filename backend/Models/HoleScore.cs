using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GolfLeagueManager
{
    public class HoleScore
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid MatchupId { get; set; }
        
        [Required]
        public int HoleNumber { get; set; }
        
        [Required]
        public int Par { get; set; }
        
        [Required]
        public int HoleHandicap { get; set; } // Stroke index for this hole (1-9, 1 being hardest)
        
        public int? PlayerAScore { get; set; }
        
        public int? PlayerBScore { get; set; }
        
        // Match play results (calculated from net scores)
        public int PlayerAMatchPoints { get; set; } // 0, 1, or 2 points for this hole
        public int PlayerBMatchPoints { get; set; } // 0, 1, or 2 points for this hole
        
        // Navigation property
        [JsonIgnore]
        public Matchup? Matchup { get; set; }
    }
}
