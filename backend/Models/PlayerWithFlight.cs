namespace GolfLeagueManager
{
    public class PlayerWithFlight
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public Guid? FlightId { get; set; }
        public string? FlightName { get; set; }
        public double? HandicapAtAssignment { get; set; }
        public bool IsFlightLeader { get; set; }
        
        // Current handicap information
        public decimal InitialHandicap { get; set; } = 0;
        public decimal CurrentHandicap { get; set; } = 0;
        
        // Average score information
        public decimal InitialAverageScore { get; set; } = 0;
        public decimal CurrentAverageScore { get; set; } = 0;
    }
}
