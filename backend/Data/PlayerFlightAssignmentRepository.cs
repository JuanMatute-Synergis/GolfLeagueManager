using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public interface IPlayerFlightAssignmentRepository
    {
        void AddAssignment(PlayerFlightAssignment assignment);
        IEnumerable<PlayerFlightAssignment> GetAssignmentsByFlight(Guid flightId);
        IEnumerable<PlayerFlightAssignment> GetAssignmentsByFlightAndSession(Guid flightId, Guid seasonId, int sessionStartWeekNumber);
        IEnumerable<PlayerFlightAssignment> GetAssignmentsByPlayer(Guid playerId);
        IEnumerable<PlayerFlightAssignment> GetAssignmentsByPlayerAndSeason(Guid playerId, Guid seasonId);
        IEnumerable<PlayerFlightAssignment> GetAssignmentsBySession(Guid seasonId, int sessionStartWeekNumber);
        PlayerFlightAssignment? GetAssignmentById(Guid id);
        PlayerFlightAssignment? GetPlayerAssignmentForSession(Guid playerId, Guid seasonId, int sessionStartWeekNumber);
        bool UpdateAssignment(PlayerFlightAssignment assignment);
        bool RemoveAssignment(Guid id);
        IEnumerable<PlayerFlightAssignment> GetAllAssignments();
    }

    public class PlayerFlightAssignmentRepository : IPlayerFlightAssignmentRepository
    {
        private readonly AppDbContext _context;

        public PlayerFlightAssignmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public void AddAssignment(PlayerFlightAssignment assignment)
        {
            _context.PlayerFlightAssignments.Add(assignment);
            _context.SaveChanges();
        }

        public IEnumerable<PlayerFlightAssignment> GetAssignmentsByFlight(Guid flightId)
        {
            return _context.PlayerFlightAssignments
                .Include(a => a.Player)
                .Include(a => a.Flight)
                .Where(a => a.FlightId == flightId)
                .OrderBy(a => a.SessionStartWeekNumber)
                .ThenBy(a => a.Player!.LastName)
                .ThenBy(a => a.Player!.FirstName)
                .ToList();
        }

        public IEnumerable<PlayerFlightAssignment> GetAssignmentsByFlightAndSession(Guid flightId, Guid seasonId, int sessionStartWeekNumber)
        {
            return _context.PlayerFlightAssignments
                .Include(a => a.Player)
                .Include(a => a.Flight)
                .Where(a => a.FlightId == flightId &&
                           a.SeasonId == seasonId &&
                           a.SessionStartWeekNumber == sessionStartWeekNumber)
                .OrderBy(a => a.Player!.LastName)
                .ThenBy(a => a.Player!.FirstName)
                .ToList();
        }

        public IEnumerable<PlayerFlightAssignment> GetAssignmentsByPlayer(Guid playerId)
        {
            return _context.PlayerFlightAssignments
                .Include(a => a.Player)
                .Include(a => a.Flight)
                .Include(a => a.Season)
                .Where(a => a.PlayerId == playerId)
                .OrderBy(a => a.Season!.Year)
                .ThenBy(a => a.SessionStartWeekNumber)
                .ThenBy(a => a.Flight!.Name)
                .ToList();
        }

        public IEnumerable<PlayerFlightAssignment> GetAssignmentsByPlayerAndSeason(Guid playerId, Guid seasonId)
        {
            return _context.PlayerFlightAssignments
                .Include(a => a.Player)
                .Include(a => a.Flight)
                .Include(a => a.Season)
                .Where(a => a.PlayerId == playerId && a.SeasonId == seasonId)
                .OrderBy(a => a.SessionStartWeekNumber)
                .ToList();
        }

        public IEnumerable<PlayerFlightAssignment> GetAssignmentsBySession(Guid seasonId, int sessionStartWeekNumber)
        {
            return _context.PlayerFlightAssignments
                .Include(a => a.Player)
                .Include(a => a.Flight)
                .Where(a => a.SeasonId == seasonId && a.SessionStartWeekNumber == sessionStartWeekNumber)
                .OrderBy(a => a.Flight!.Name)
                .ThenBy(a => a.Player!.LastName)
                .ThenBy(a => a.Player!.FirstName)
                .ToList();
        }

        public PlayerFlightAssignment? GetPlayerAssignmentForSession(Guid playerId, Guid seasonId, int sessionStartWeekNumber)
        {
            return _context.PlayerFlightAssignments
                .Include(a => a.Player)
                .Include(a => a.Flight)
                .FirstOrDefault(a => a.PlayerId == playerId &&
                               a.SeasonId == seasonId &&
                               a.SessionStartWeekNumber == sessionStartWeekNumber);
        }

        public PlayerFlightAssignment? GetAssignmentById(Guid id)
        {
            return _context.PlayerFlightAssignments
                .Include(a => a.Player)
                .Include(a => a.Flight)
                .FirstOrDefault(a => a.Id == id);
        }

        public bool UpdateAssignment(PlayerFlightAssignment assignment)
        {
            var existingAssignment = _context.PlayerFlightAssignments.Find(assignment.Id);
            if (existingAssignment == null)
                return false;

            existingAssignment.PlayerId = assignment.PlayerId;
            existingAssignment.FlightId = assignment.FlightId;
            existingAssignment.SeasonId = assignment.SeasonId;
            existingAssignment.SessionStartWeekNumber = assignment.SessionStartWeekNumber;
            existingAssignment.IsFlightLeader = assignment.IsFlightLeader;
            existingAssignment.HandicapAtAssignment = assignment.HandicapAtAssignment;

            _context.SaveChanges();
            return true;
        }

        public bool RemoveAssignment(Guid id)
        {
            var assignment = _context.PlayerFlightAssignments.Find(id);
            if (assignment == null)
                return false;

            _context.PlayerFlightAssignments.Remove(assignment);
            _context.SaveChanges();
            return true;
        }

        public IEnumerable<PlayerFlightAssignment> GetAllAssignments()
        {
            return _context.PlayerFlightAssignments
                .Include(a => a.Player)
                .Include(a => a.Flight)
                .ToList();
        }
    }
}
