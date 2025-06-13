namespace GolfLeagueManager
{
    public class Matchup
    {
        public Guid Id { get; set; }
        public Guid WeekId { get; set; }
        public Guid PlayerAId { get; set; }
        public Guid PlayerBId { get; set; }
        public int? PlayerAScore { get; set; }
        public int? PlayerBScore { get; set; }
        public int? PlayerAPoints { get; set; }
        public int? PlayerBPoints { get; set; }
        
        // Navigation properties
        public Week Week { get; set; } = null!;
        public Player PlayerA { get; set; } = null!;
        public Player PlayerB { get; set; } = null!;
    }
}
