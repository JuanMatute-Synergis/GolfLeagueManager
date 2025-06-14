namespace GolfLeagueManager
{
    public class Week
    {
        public Guid Id { get; set; }
        public int WeekNumber { get; set; } // e.g. 1, 2, 3, etc.
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Name { get; set; } = string.Empty; // e.g. "Week 1", "Championship Week"
        public bool IsActive { get; set; } = true;
        
        // Foreign key to Season
        public Guid SeasonId { get; set; }
        public Season Season { get; set; } = null!;
        
        // Navigation property for matchups in this week
        public List<Matchup> Matchups { get; set; } = new List<Matchup>();

        // Navigation property for score entries in this week
        public List<ScoreEntry> ScoreEntries { get; set; } = new List<ScoreEntry>();
    }
}
