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
    }
}
