using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public interface IFlightRepository
    {
        void AddFlight(Flight flight);
        IEnumerable<Flight> GetFlights();
        Flight? GetFlightById(Guid id);
        bool DeleteFlight(Guid id);
        bool UpdateFlight(Flight flight);
        IEnumerable<Flight> GetActiveFlights();
        IEnumerable<Flight> GetFlightsBySeason(Guid seasonId);
    }

    public class FlightRepository : IFlightRepository
    {
        private readonly AppDbContext _context;

        public FlightRepository(AppDbContext context)
        {
            _context = context;
        }

        public void AddFlight(Flight flight)
        {
            flight.CreatedAt = DateTime.UtcNow;
            flight.UpdatedAt = DateTime.UtcNow;
            _context.Flights.Add(flight);
            _context.SaveChanges();
        }

        public IEnumerable<Flight> GetFlights()
        {
            return _context.Flights.OrderBy(f => f.Name).ToList();
        }

        public Flight? GetFlightById(Guid id)
        {
            return _context.Flights.Find(id);
        }

        public bool DeleteFlight(Guid id)
        {
            var flight = _context.Flights.Find(id);
            if (flight == null)
                return false;
            
            _context.Flights.Remove(flight);
            _context.SaveChanges();
            return true;
        }

        public bool UpdateFlight(Flight flight)
        {
            var existingFlight = _context.Flights.Find(flight.Id);
            if (existingFlight == null)
                return false;

            existingFlight.Name = flight.Name;
            existingFlight.MaxPlayers = flight.MaxPlayers;
            existingFlight.Description = flight.Description;
            existingFlight.IsActive = flight.IsActive;
            existingFlight.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
            return true;
        }

        public IEnumerable<Flight> GetActiveFlights()
        {
            return _context.Flights
                .Where(f => f.IsActive)
                .OrderBy(f => f.Name)
                .ToList();
        }

        public IEnumerable<Flight> GetFlightsBySeason(Guid seasonId)
        {
            return _context.Flights
                .Where(f => f.SeasonId == seasonId)
                .OrderBy(f => f.Name)
                .ToList();
        }
    }
}
