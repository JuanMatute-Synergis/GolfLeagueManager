namespace GolfLeagueManager
{
    public class PlayerFlightAssignment
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public Guid FlightId { get; set; }
        public bool IsFlightLeader { get; set; }
        public double HandicapAtAssignment { get; set; }
        
        // Navigation properties
        public Player Player { get; set; } = null!;
        public Flight Flight { get; set; } = null!;
    }
}