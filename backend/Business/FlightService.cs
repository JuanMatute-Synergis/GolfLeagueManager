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

        public IEnumerable<Flight> GetUpcomingFlights()
        {
            var today = DateTime.Today;
            return _flightRepository.GetFlightsByDateRange(today, today.AddMonths(3))
                .Where(f => f.IsActive);
        }

        public IEnumerable<Flight> GetFlightsByDateRange(DateTime startDate, DateTime endDate)
        {
            return _flightRepository.GetFlightsByDateRange(startDate, endDate);
        }

        private void ValidateFlight(Flight flight)
        {
            if (string.IsNullOrWhiteSpace(flight.Name))
                throw new ArgumentException("Flight name is required.");

            if (string.IsNullOrWhiteSpace(flight.Course))
                throw new ArgumentException("Course is required.");

            if (flight.MaxPlayers <= 0)
                throw new ArgumentException("Max players must be greater than 0.");

            if (flight.Date < DateTime.Today)
                throw new ArgumentException("Flight date cannot be in the past.");
        }
    }
}
