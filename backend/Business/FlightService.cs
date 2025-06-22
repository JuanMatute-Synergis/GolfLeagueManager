namespace GolfLeagueManager
{
    public class FlightService
    {
        private readonly IFlightRepository _flightRepository;

        public FlightService(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public void AddFlight(Flight flight)
        {
            // Business logic validation
            ValidateFlight(flight);
            _flightRepository.AddFlight(flight);
        }

        public IEnumerable<Flight> GetFlights()
        {
            return _flightRepository.GetFlights();
        }

        public Flight? GetFlightById(Guid id)
        {
            return _flightRepository.GetFlightById(id);
        }

        public bool DeleteFlight(Guid id)
        {
            return _flightRepository.DeleteFlight(id);
        }

        public bool UpdateFlight(Flight flight)
        {
            ValidateFlight(flight);
            return _flightRepository.UpdateFlight(flight);
        }

        public IEnumerable<Flight> GetActiveFlights()
        {
            return _flightRepository.GetActiveFlights();
        }

        public IEnumerable<Flight> GetFlightsBySeason(Guid seasonId)
        {
            return _flightRepository.GetFlightsBySeason(seasonId);
        }

        public async Task<List<Flight>> GetFlightsBySeasonIdAsync(Guid seasonId)
        {
            // Use the repository, not _context
            return await Task.Run(() => _flightRepository.GetFlightsBySeason(seasonId).ToList());
        }

        private void ValidateFlight(Flight flight)
        {
            if (string.IsNullOrWhiteSpace(flight.Name))
                throw new ArgumentException("Flight name is required.");

            if (flight.MaxPlayers <= 0)
                throw new ArgumentException("Max players must be greater than 0.");
        }
    }
}
