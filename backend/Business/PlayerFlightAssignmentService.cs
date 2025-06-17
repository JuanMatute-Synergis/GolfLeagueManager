namespace GolfLeagueManager
{
    public class PlayerFlightAssignmentService
    {
        private readonly IPlayerFlightAssignmentRepository _repository;

        public PlayerFlightAssignmentService(IPlayerFlightAssignmentRepository repository)
        {
            _repository = repository;
        }

        public void AddAssignment(PlayerFlightAssignment assignment)
        {
            if (assignment.Id == Guid.Empty)
            {
                assignment.Id = Guid.NewGuid();
            }
            ValidateAssignment(assignment);
            _repository.AddAssignment(assignment);
        }

        public IEnumerable<PlayerFlightAssignment> GetAssignmentsByFlight(Guid flightId)
        {
            return _repository.GetAssignmentsByFlight(flightId);
        }

        public IEnumerable<PlayerFlightAssignment> GetAssignmentsByPlayer(Guid playerId)
        {
            return _repository.GetAssignmentsByPlayer(playerId);
        }

        public PlayerFlightAssignment? GetAssignmentById(Guid id)
        {
            return _repository.GetAssignmentById(id);
        }

        public bool UpdateAssignment(PlayerFlightAssignment assignment)
        {
            ValidateAssignment(assignment);
            return _repository.UpdateAssignment(assignment);
        }

        public bool RemoveAssignment(Guid id)
        {
            return _repository.RemoveAssignment(id);
        }

        public IEnumerable<PlayerFlightAssignment> GetAllAssignments()
        {
            return _repository.GetAllAssignments();
        }

        private void ValidateAssignment(PlayerFlightAssignment assignment)
        {
            if (assignment.PlayerId == Guid.Empty)
                throw new ArgumentException("Player ID is required.");

            if (assignment.FlightId == Guid.Empty)
                throw new ArgumentException("Flight ID is required.");

            if (assignment.HandicapAtAssignment < 0)
                throw new ArgumentException("Handicap cannot be negative.");
        }
    }
}
