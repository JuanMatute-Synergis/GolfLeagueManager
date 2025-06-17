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
        
        public int? PlayerAScore { get; set; }
        
        public int? PlayerBScore { get; set; }
        
        // Navigation property
        [JsonIgnore]
        public Matchup? Matchup { get; set; }
    }
}
