using System.Collections.Generic;
using System.Linq;

namespace GolfLeagueManager
{
    public class PlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly PlayerFlightAssignmentService _assignmentService;
        private readonly FlightService _flightService;

        public PlayerService(IPlayerRepository playerRepository, PlayerFlightAssignmentService assignmentService, FlightService flightService)
        {
            _playerRepository = playerRepository;
            _assignmentService = assignmentService;
            _flightService = flightService;
        }

        public void AddPlayer(Player player)
        {
            if (player.Id == Guid.Empty)
            {
                player.Id = Guid.NewGuid();
            }
            _playerRepository.AddPlayer(player);
        }

        public IEnumerable<Player> GetPlayers()
        {
            return _playerRepository.GetPlayers();
        }

        public bool DeletePlayer(Guid id)
        {
            return _playerRepository.DeletePlayer(id);
        }

        public bool UpdatePlayer(Player player)
        {
            return _playerRepository.UpdatePlayer(player);
        }

        public IEnumerable<PlayerWithFlight> GetPlayersInFlights(Guid seasonId)
        {
            // Get all flights for the season
            var seasonFlights = _flightService.GetFlightsBySeason(seasonId);
            var flightIds = seasonFlights.Select(f => f.Id).ToList();
            
            // Get all assignments for these flights
            var playersWithFlights = new List<PlayerWithFlight>();
            foreach (var flight in seasonFlights)
            {
                var assignments = _assignmentService.GetAssignmentsByFlight(flight.Id);
                foreach (var assignment in assignments)
                {
                    var player = _playerRepository.GetPlayers().FirstOrDefault(p => p.Id == assignment.PlayerId);
                    if (player != null)
                    {
                        playersWithFlights.Add(new PlayerWithFlight
                        {
                            Id = player.Id,
                            FirstName = player.FirstName,
                            LastName = player.LastName,
                            Email = player.Email,
                            Phone = player.Phone,
                            FlightId = flight.Id,
                            FlightName = flight.Name,
                            HandicapAtAssignment = assignment.HandicapAtAssignment,
                            IsFlightLeader = assignment.IsFlightLeader,
                            InitialHandicap = player.InitialHandicap,
                            CurrentHandicap = player.CurrentHandicap,
                            InitialAverageScore = player.InitialAverageScore,
                            CurrentAverageScore = player.CurrentAverageScore
                        });
                    }
                }
            }
            
            return playersWithFlights;
        }

        // Additional business logic methods can be added here
    }

    // IDataAccess and InMemoryDataAccess are now obsolete and can be removed if not needed for testing.
}
