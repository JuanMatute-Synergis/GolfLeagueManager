using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GolfLeagueManager
{
    public class Matchup
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid WeekId { get; set; }
        
        [Required]
        public Guid PlayerAId { get; set; }
        
        [Required]
        public Guid PlayerBId { get; set; }
        
        public int? PlayerAScore { get; set; }
        public int? PlayerBScore { get; set; }
        public int? PlayerAPoints { get; set; } // Total match play points (0-20)
        public int? PlayerBPoints { get; set; } // Total match play points (0-20)
        
        // Match play breakdown
        public int PlayerAHolePoints { get; set; } // Points from individual holes (0-18)
        public int PlayerBHolePoints { get; set; } // Points from individual holes (0-18)
        public bool PlayerAMatchWin { get; set; } // Gets the 2-point match bonus
        public bool PlayerBMatchWin { get; set; } // Gets the 2-point match bonus
        
        // Navigation properties - nullable for JSON serialization
        [JsonIgnore]
        public Week? Week { get; set; }
        
        [JsonIgnore]
        public Player? PlayerA { get; set; }
        
        [JsonIgnore]
        public Player? PlayerB { get; set; }
    }
}
