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
