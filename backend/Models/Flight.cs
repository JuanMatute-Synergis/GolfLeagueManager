using System.Text.Json.Serialization;

namespace GolfLeagueManager
{
    public class Flight
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MaxPlayers { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign key to Season (optional - flights can exist without being assigned to a season)
        public Guid? SeasonId { get; set; }
        [JsonIgnore]
        public Season? Season { get; set; }
    }
}
