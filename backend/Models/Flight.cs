namespace GolfLeagueManager
{
    public class Flight
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public string Course { get; set; } = string.Empty;
        public int MaxPlayers { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign key to Week (optional - flights can exist without being assigned to a week)
        public Guid? WeekId { get; set; }
        public Week? Week { get; set; }
    }
}
