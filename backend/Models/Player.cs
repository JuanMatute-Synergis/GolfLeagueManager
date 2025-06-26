using System.Text.Json.Serialization;

namespace GolfLeagueManager
{
    public class Player
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // Player avatar or profile picture (URL or path)
        public string? ImageUrl { get; set; }
        
        // Handicap system
        public decimal InitialHandicap { get; set; } = 0; // Starting handicap for the season
        public decimal CurrentHandicap { get; set; } = 0; // Current calculated handicap
        
        // Average score system
        public decimal InitialAverageScore { get; set; } = 0; // Starting average score for the season
        public decimal CurrentAverageScore { get; set; } = 0; // Current calculated average score
        
        // Navigation properties for matchups where this player is involved
        [JsonIgnore]
        public List<Matchup> MatchupsAsPlayerA { get; set; } = new List<Matchup>();
        [JsonIgnore]
        public List<Matchup> MatchupsAsPlayerB { get; set; } = new List<Matchup>();
        
        // Navigation properties for flight assignments
        [JsonIgnore]
        public List<PlayerFlightAssignment> FlightAssignments { get; set; } = new List<PlayerFlightAssignment>();
        
        // Navigation properties for score entries
        [JsonIgnore]
        public List<ScoreEntry> ScoreEntries { get; set; } = new List<ScoreEntry>();
    }
}
