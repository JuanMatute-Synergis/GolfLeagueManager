namespace GolfLeagueManager
{
    public class ScoreEntry
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public Guid WeekId { get; set; }
        public int Score { get; set; }
        public int PointsEarned { get; set; }
        
        // Navigation properties
        public Player Player { get; set; } = null!;
        public Week Week { get; set; } = null!;
    }
}