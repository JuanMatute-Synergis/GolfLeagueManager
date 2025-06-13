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
        IEnumerable<Flight> GetFlightsByDateRange(DateTime startDate, DateTime endDate);
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
            // Ensure the Date is in UTC
            if (flight.Date.Kind == DateTimeKind.Unspecified)
            {
                flight.Date = DateTime.SpecifyKind(flight.Date, DateTimeKind.Utc);
            }
            else if (flight.Date.Kind == DateTimeKind.Local)
            {
                flight.Date = flight.Date.ToUniversalTime();
            }

            flight.CreatedAt = DateTime.UtcNow;
            flight.UpdatedAt = DateTime.UtcNow;
            _context.Flights.Add(flight);
            _context.SaveChanges();
        }

        public IEnumerable<Flight> GetFlights()
        {
            return _context.Flights.OrderBy(f => f.Date).ThenBy(f => f.StartTime).ToList();
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

            // Ensure the Date is in UTC
            var updatedDate = flight.Date;
            if (updatedDate.Kind == DateTimeKind.Unspecified)
            {
                updatedDate = DateTime.SpecifyKind(updatedDate, DateTimeKind.Utc);
            }
            else if (updatedDate.Kind == DateTimeKind.Local)
            {
                updatedDate = updatedDate.ToUniversalTime();
            }

            existingFlight.Name = flight.Name;
            existingFlight.Date = updatedDate;
            existingFlight.StartTime = flight.StartTime;
            existingFlight.Course = flight.Course;
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
                .OrderBy(f => f.Date)
                .ThenBy(f => f.StartTime)
                .ToList();
        }

        public IEnumerable<Flight> GetFlightsByDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.Flights
                .Where(f => f.Date >= startDate && f.Date <= endDate)
                .OrderBy(f => f.Date)
                .ThenBy(f => f.StartTime)
                .ToList();
        }
    }
}
