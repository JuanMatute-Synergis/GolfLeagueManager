using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GolfLeagueManager.Business;

namespace GolfLeagueManager
{
    public class PlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly PlayerFlightAssignmentService _assignmentService;
        private readonly FlightService _flightService;
        private readonly PlayerSeasonStatsService _playerSeasonStatsService;

        public PlayerService(IPlayerRepository playerRepository, PlayerFlightAssignmentService assignmentService, FlightService flightService, PlayerSeasonStatsService playerSeasonStatsService)
        {
            _playerRepository = playerRepository;
            _assignmentService = assignmentService;
            _flightService = flightService;
            _playerSeasonStatsService = playerSeasonStatsService;
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

        public async Task<IEnumerable<PlayerWithFlight>> GetPlayersInFlightsAsync(Guid seasonId)
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
                        // Get season-specific data for this player
                        var initialHandicap = await _playerSeasonStatsService.GetInitialHandicapAsync(player.Id, seasonId);
                        var initialAverageScore = await _playerSeasonStatsService.GetInitialAverageScoreAsync(player.Id, seasonId);
                        var currentAverageScore = await _playerSeasonStatsService.GetCurrentAverageScoreAsync(player.Id, seasonId);

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
                            InitialHandicap = initialHandicap,
                            // Don't include CurrentHandicap as it should always be calculated on demand
                            InitialAverageScore = initialAverageScore,
                            CurrentAverageScore = currentAverageScore
                        });
                    }
                }
            }

            return playersWithFlights;
        }

        public async Task<List<Player>> GetAllPlayersAsync()
        {
            // Use the repository, not _context
            return await Task.Run(() => _playerRepository.GetPlayers().ToList());
        }

        // Additional business logic methods can be added here
    }

    // IDataAccess and InMemoryDataAccess are now obsolete and can be removed if not needed for testing.
}
