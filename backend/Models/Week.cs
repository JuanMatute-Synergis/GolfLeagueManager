using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GolfLeagueManager
{
    public class Week
    {
        public Guid Id { get; set; }
        public int WeekNumber { get; set; } // e.g. 1, 2, 3, etc.
        public DateTime Date { get; set; } // The Wednesday date when the week is played
        public string Name { get; set; } = string.Empty; // e.g. "Week 1", "Championship Week"
        public bool IsActive { get; set; } = true;
        
        // Scoring and handicap calculation toggles
        public bool CountsForScoring { get; set; } = true; // Whether this week counts for scoring
        public bool CountsForHandicap { get; set; } = true; // Whether this week counts for handicap calculation
        
        // Foreign key to Season
        [Required]
        public Guid SeasonId { get; set; }
        
        [JsonIgnore]
        public Season? Season { get; set; }
        
        // Navigation property for matchups in this week
        [JsonIgnore]
        public List<Matchup> Matchups { get; set; } = new List<Matchup>();

        // Navigation property for score entries in this week
        [JsonIgnore]
        public List<ScoreEntry> ScoreEntries { get; set; } = new List<ScoreEntry>();
    }
}
