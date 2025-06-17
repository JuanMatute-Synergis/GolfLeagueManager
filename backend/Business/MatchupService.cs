using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public class MatchupService
    {
        private readonly IMatchupRepository _matchupRepository;
        private readonly PlayerFlightAssignmentService _playerFlightAssignmentService;
        private readonly AppDbContext _context;

        public MatchupService(IMatchupRepository matchupRepository, PlayerFlightAssignmentService playerFlightAssignmentService, AppDbContext context)
        {
            _matchupRepository = matchupRepository;
            _playerFlightAssignmentService = playerFlightAssignmentService;
            _context = context;
        }

        public async Task<IEnumerable<Matchup>> GetAllMatchupsAsync()
        {
            return await _matchupRepository.GetAllAsync();
        }

        public async Task<Matchup?> GetMatchupByIdAsync(Guid id)
        {
            return await _matchupRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Matchup>> GetMatchupsByWeekIdAsync(Guid weekId)
        {
            var matchups = await _matchupRepository.GetByWeekIdAsync(weekId);
            
            // Calculate total scores for each matchup
            foreach (var matchup in matchups)
            {
                await CalculateMatchupTotalsAsync(matchup);
            }
            
            return matchups;
        }

        public async Task<IEnumerable<Matchup>> GetMatchupsBySeasonIdAsync(Guid seasonId)
        {
            return await _matchupRepository.GetBySeasonIdAsync(seasonId);
        }

        public async Task<IEnumerable<Matchup>> GetMatchupsByPlayerIdAsync(Guid playerId)
        {
            return await _matchupRepository.GetByPlayerIdAsync(playerId);
        }

        public async Task<Matchup> CreateMatchupAsync(Matchup matchup)
        {
            // Validate that both players are in the same flight
            // ValidatePlayersInSameFlight(matchup.PlayerAId, matchup.PlayerBId);
            
            // Validate that players don't already have a matchup for this week
            await ValidatePlayersNotAlreadyMatchedForWeek(matchup.WeekId, matchup.PlayerAId, matchup.PlayerBId);
            
            return await _matchupRepository.CreateAsync(matchup);
        }

        public async Task<Matchup> UpdateMatchupAsync(Matchup matchup)
        {
            // Validate that both players are in the same flight
            // ValidatePlayersInSameFlight(matchup.PlayerAId, matchup.PlayerBId);
            
            // Validate that players don't already have a matchup for this week (excluding current matchup)
            await ValidatePlayersNotAlreadyMatchedForWeekExcludingCurrent(matchup.WeekId, matchup.PlayerAId, matchup.PlayerBId, matchup.Id);
            
            return await _matchupRepository.UpdateAsync(matchup);
        }

        public async Task<bool> DeleteMatchupAsync(Guid id)
        {
            return await _matchupRepository.DeleteAsync(id);
        }

        private void ValidatePlayersInSameFlight(Guid playerAId, Guid playerBId)
        {
            var playerAAssignments = _playerFlightAssignmentService.GetAssignmentsByPlayer(playerAId);
            var playerBAssignments = _playerFlightAssignmentService.GetAssignmentsByPlayer(playerBId);

            // Check if players share any flight
            var sharedFlights = playerAAssignments.Select(a => a.FlightId)
                .Intersect(playerBAssignments.Select(a => a.FlightId));

            if (!sharedFlights.Any())
            {
                throw new InvalidOperationException("Players must be in the same flight to create a matchup.");
            }
        }

        private async Task ValidatePlayersNotAlreadyMatchedForWeek(Guid weekId, Guid playerAId, Guid playerBId)
        {
            var existingMatchupsForWeek = await _matchupRepository.GetByWeekIdAsync(weekId);
            
            // Check if Player A already has a matchup this week
            var playerAExistingMatchup = existingMatchupsForWeek.FirstOrDefault(m => 
                m.PlayerAId == playerAId || m.PlayerBId == playerAId);
            
            if (playerAExistingMatchup != null)
            {
                throw new InvalidOperationException($"Player A already has a matchup for this week (Matchup ID: {playerAExistingMatchup.Id}).");
            }
            
            // Check if Player B already has a matchup this week
            var playerBExistingMatchup = existingMatchupsForWeek.FirstOrDefault(m => 
                m.PlayerAId == playerBId || m.PlayerBId == playerBId);
            
            if (playerBExistingMatchup != null)
            {
                throw new InvalidOperationException($"Player B already has a matchup for this week (Matchup ID: {playerBExistingMatchup.Id}).");
            }
        }

        private async Task ValidatePlayersNotAlreadyMatchedForWeekExcludingCurrent(Guid weekId, Guid playerAId, Guid playerBId, Guid currentMatchupId)
        {
            var existingMatchupsForWeek = await _matchupRepository.GetByWeekIdAsync(weekId);
            
            // Check if Player A already has a matchup this week (excluding the current one being updated)
            var playerAExistingMatchup = existingMatchupsForWeek.FirstOrDefault(m => 
                m.Id != currentMatchupId && (m.PlayerAId == playerAId || m.PlayerBId == playerAId));
            
            if (playerAExistingMatchup != null)
            {
                throw new InvalidOperationException($"Player A already has a matchup for this week (Matchup ID: {playerAExistingMatchup.Id}).");
            }
            
            // Check if Player B already has a matchup this week (excluding the current one being updated)
            var playerBExistingMatchup = existingMatchupsForWeek.FirstOrDefault(m => 
                m.Id != currentMatchupId && (m.PlayerAId == playerBId || m.PlayerBId == playerBId));
            
            if (playerBExistingMatchup != null)
            {
                throw new InvalidOperationException($"Player B already has a matchup for this week (Matchup ID: {playerBExistingMatchup.Id}).");
            }
        }

        public async Task<IEnumerable<Matchup>> GenerateRandomMatchupsForWeekAsync(Guid weekId, Guid seasonId)
        {
            // Clear existing matchups for this week first
            var existingMatchups = await _matchupRepository.GetByWeekIdAsync(weekId);
            foreach (var existingMatchup in existingMatchups)
            {
                await _matchupRepository.DeleteAsync(existingMatchup.Id);
            }

            // Get all players assigned to flights for this season
            var playerAssignments = _playerFlightAssignmentService.GetAllAssignments()
                .GroupBy(a => a.FlightId)
                .ToList();

            var matchups = new List<Matchup>();

            foreach (var flightGroup in playerAssignments)
            {
                var playersInFlight = flightGroup.Select(a => a.PlayerId).ToList();
                
                // Generate single matchups for this flight (each player plays once)
                var flightMatchups = GenerateOneMatchupPerPlayer(playersInFlight, weekId);
                
                foreach (var matchup in flightMatchups)
                {
                    matchups.Add(await _matchupRepository.CreateAsync(matchup));
                }
            }

            return matchups;
        }

        private List<Matchup> GenerateOneMatchupPerPlayer(List<Guid> players, Guid weekId)
        {
            var matchups = new List<Matchup>();
            var availablePlayers = new List<Guid>(players);
            var random = new Random();

            // Shuffle the players for random pairing
            for (int i = availablePlayers.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (availablePlayers[i], availablePlayers[j]) = (availablePlayers[j], availablePlayers[i]);
            }

            // Pair up players (if odd number, one player sits out)
            for (int i = 0; i < availablePlayers.Count - 1; i += 2)
            {
                var matchup = new Matchup
                {
                    WeekId = weekId,
                    PlayerAId = availablePlayers[i],
                    PlayerBId = availablePlayers[i + 1]
                };
                
                matchups.Add(matchup);
            }

            return matchups;
        }

        private async Task CalculateMatchupTotalsAsync(Matchup matchup)
        {
            // Get all hole scores for this matchup
            var holeScores = await _context.HoleScores
                .Where(hs => hs.MatchupId == matchup.Id)
                .ToListAsync();

            if (holeScores.Any())
            {
                // Calculate total scores
                matchup.PlayerAScore = holeScores
                    .Sum(hs => hs.PlayerAScore ?? 0);

                matchup.PlayerBScore = holeScores
                    .Sum(hs => hs.PlayerBScore ?? 0);
            }
        }
    }
}
