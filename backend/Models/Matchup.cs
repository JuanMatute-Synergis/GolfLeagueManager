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
        public int? PlayerAPoints { get; set; }
        public int? PlayerBPoints { get; set; }
        
        // Navigation properties - nullable for JSON serialization
        [JsonIgnore]
        public Week? Week { get; set; }
        
        [JsonIgnore]
        public Player? PlayerA { get; set; }
        
        [JsonIgnore]
        public Player? PlayerB { get; set; }
    }
}
