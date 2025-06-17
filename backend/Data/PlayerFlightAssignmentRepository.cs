using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public interface IPlayerFlightAssignmentRepository
    {
        void AddAssignment(PlayerFlightAssignment assignment);
        IEnumerable<PlayerFlightAssignment> GetAssignmentsByFlight(Guid flightId);
        IEnumerable<PlayerFlightAssignment> GetAssignmentsByPlayer(Guid playerId);
        PlayerFlightAssignment? GetAssignmentById(Guid id);
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
                .OrderBy(a => a.Player!.LastName)
                .ThenBy(a => a.Player!.FirstName)
                .ToList();
        }

        public IEnumerable<PlayerFlightAssignment> GetAssignmentsByPlayer(Guid playerId)
        {
            return _context.PlayerFlightAssignments
                .Include(a => a.Player)
                .Include(a => a.Flight)
                .Where(a => a.PlayerId == playerId)
                .OrderBy(a => a.Flight!.Name)
                .ToList();
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
