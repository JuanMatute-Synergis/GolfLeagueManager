using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public class MatchPlayService
    {
        private readonly AppDbContext _context;
        private readonly MatchPlayScoringService _scoringService;

        public MatchPlayService(AppDbContext context, MatchPlayScoringService scoringService)
        {
            _context = context;
            _scoringService = scoringService;
        }

        /// <summary>
        /// Calculate match play results for a matchup based on hole-by-hole scores
        /// </summary>
        public async Task<bool> CalculateMatchPlayResultsAsync(Guid matchupId)
        {
            // Get matchup with players and hole scores
            var matchup = await _context.Matchups
                .Include(m => m.PlayerA)
                .Include(m => m.PlayerB)
                .FirstOrDefaultAsync(m => m.Id == matchupId);

            if (matchup == null) return false;

            // Handle absence scenarios first
            if (matchup.PlayerAAbsent || matchup.PlayerBAbsent)
            {
                return await CalculateAbsenceScenarioAsync(matchup);
            }

            var holeScores = await _context.HoleScores
                .Where(hs => hs.MatchupId == matchupId)
                .OrderBy(hs => hs.HoleNumber)
                .ToListAsync();

            if (!holeScores.Any()) return false;

            // Get current handicaps for both players
            var playerAHandicap = matchup.PlayerA?.CurrentHandicap ?? 0;
            var playerBHandicap = matchup.PlayerB?.CurrentHandicap ?? 0;

            // Calculate gross totals for tie-breaking
            var playerAGrossTotal = holeScores.Where(hs => hs.PlayerAScore.HasValue).Sum(hs => hs.PlayerAScore!.Value);
            var playerBGrossTotal = holeScores.Where(hs => hs.PlayerBScore.HasValue).Sum(hs => hs.PlayerBScore!.Value);

            // Use the new match play scoring service
            var matchPlayResult = _scoringService.CalculateMatchPlayResult(
                holeScores, 
                playerAHandicap, 
                playerBHandicap,
                playerAGrossTotal,
                playerBGrossTotal);

            // Update hole scores with match play points
            for (int i = 0; i < holeScores.Count; i++)
            {
                var holeResult = matchPlayResult.HoleResults.FirstOrDefault(hr => hr.HoleNumber == holeScores[i].HoleNumber);
                if (holeResult != null)
                {
                    holeScores[i].PlayerAMatchPoints = holeResult.PlayerAPoints;
                    holeScores[i].PlayerBMatchPoints = holeResult.PlayerBPoints;
                }
            }

            // Update matchup with match play results
            matchup.PlayerAHolePoints = matchPlayResult.PlayerAHolePoints;
            matchup.PlayerBHolePoints = matchPlayResult.PlayerBHolePoints;
            matchup.PlayerAPoints = matchPlayResult.PlayerATotalPoints;
            matchup.PlayerBPoints = matchPlayResult.PlayerBTotalPoints;
            matchup.PlayerAMatchWin = matchPlayResult.PlayerAMatchWin;
            matchup.PlayerBMatchWin = matchPlayResult.PlayerBMatchWin;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Calculate points for absence scenarios
        /// </summary>
        private async Task<bool> CalculateAbsenceScenarioAsync(Matchup matchup)
        {
            if (matchup.PlayerAAbsent && matchup.PlayerBAbsent)
            {
                // Both absent - distribute points based on notice
                if (matchup.PlayerAAbsentWithNotice && matchup.PlayerBAbsentWithNotice)
                {
                    // Both gave notice - split points evenly
                    matchup.PlayerAPoints = 10;
                    matchup.PlayerBPoints = 10;
                }
                else if (matchup.PlayerAAbsentWithNotice)
                {
                    // Only A gave notice
                    matchup.PlayerAPoints = 4;
                    matchup.PlayerBPoints = 0;
                    // Remaining 16 points are forfeited
                }
                else if (matchup.PlayerBAbsentWithNotice)
                {
                    // Only B gave notice
                    matchup.PlayerAPoints = 0;
                    matchup.PlayerBPoints = 4;
                    // Remaining 16 points are forfeited
                }
                else
                {
                    // Neither gave notice
                    matchup.PlayerAPoints = 0;
                    matchup.PlayerBPoints = 0;
                    // All 20 points are forfeited
                }
                matchup.PlayerAHolePoints = 0;
                matchup.PlayerBHolePoints = 0;
                matchup.PlayerAMatchWin = false;
                matchup.PlayerBMatchWin = false;
            }
            else if (matchup.PlayerAAbsent)
            {
                // Player A absent, Player B present
                var playerAPoints = matchup.PlayerAAbsentWithNotice ? 4 : 0;
                var playerBHandicap = await GetPlayerHandicapAsync(matchup.PlayerBId);
                var playerBPoints = await CalculateNoOpponentScoringAsync(matchup, matchup.PlayerBId, playerBHandicap);
                
                matchup.PlayerAPoints = playerAPoints;
                matchup.PlayerBPoints = playerBPoints;
                matchup.PlayerAHolePoints = 0;
                matchup.PlayerBHolePoints = Math.Max(0, playerBPoints - 2); // Subtract match bonus to get hole points
                matchup.PlayerAMatchWin = false;
                matchup.PlayerBMatchWin = playerBPoints > playerAPoints;
            }
            else if (matchup.PlayerBAbsent)
            {
                // Player B absent, Player A present
                var playerBPoints = matchup.PlayerBAbsentWithNotice ? 4 : 0;
                var playerAHandicap = await GetPlayerHandicapAsync(matchup.PlayerAId);
                var playerAPoints = await CalculateNoOpponentScoringAsync(matchup, matchup.PlayerAId, playerAHandicap);
                
                matchup.PlayerAPoints = playerAPoints;
                matchup.PlayerBPoints = playerBPoints;
                matchup.PlayerAHolePoints = Math.Max(0, playerAPoints - 2); // Subtract match bonus to get hole points
                matchup.PlayerBHolePoints = 0;
                matchup.PlayerAMatchWin = playerAPoints > playerBPoints;
                matchup.PlayerBMatchWin = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Calculate points when player has no opponent (absence scenario)
        /// Rules: Present player gets 16 points if they beat their average by a whole number, otherwise 8 points
        /// </summary>
        private async Task<int> CalculateNoOpponentScoringAsync(Matchup matchup, Guid playerId, double handicap)
        {
            // Get the player's current average score
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
            {
                return 8; // Default fallback if player not found
            }

            // Get hole scores for this matchup to calculate total score
            var holeScores = await _context.HoleScores
                .Where(hs => hs.MatchupId == matchup.Id)
                .OrderBy(hs => hs.HoleNumber)
                .ToListAsync();

            if (!holeScores.Any())
            {
                // No hole scores available, award minimum points
                return 8;
            }

            // Calculate the player's total gross score
            int totalScore = 0;
            bool hasValidScore = false;
            
            foreach (var holeScore in holeScores)
            {
                var actualScore = playerId == matchup.PlayerAId ? holeScore.PlayerAScore : holeScore.PlayerBScore;
                
                if (actualScore.HasValue)
                {
                    totalScore += actualScore.Value;
                    hasValidScore = true;
                }
            }

            if (!hasValidScore)
            {
                // No valid scores found, award minimum points
                return 8;
            }

            // Check if player beat their average by a whole number
            // Example: If average is 43.99, they need to shoot 42 or better
            var averageScore = player.CurrentAverageScore;
            var requiredScore = Math.Floor(averageScore); // This gives us the whole number threshold
            
            if (totalScore < requiredScore)
            {
                // Player beat their average by a whole number - award 16 points
                return 16;
            }
            else
            {
                // Player did not beat their average by a whole number - award 8 points
                return 8;
            }
        }

        /// <summary>
        /// Get player's current handicap from their flight assignment
        /// </summary>
        private async Task<double> GetPlayerHandicapAsync(Guid playerId)
        {
            var assignment = await _context.PlayerFlightAssignments
                .Where(pfa => pfa.PlayerId == playerId)
                .OrderByDescending(pfa => pfa.Id) // Get most recent assignment
                .FirstOrDefaultAsync();

            return assignment?.HandicapAtAssignment ?? 0.0;
        }

        /// <summary>
        /// Initialize hole scores with default hole handicaps (stroke index)
        /// This assumes a standard 9-hole course with typical stroke index distribution
        /// </summary>
        public async Task<bool> InitializeHoleScoresAsync(Guid matchupId)
        {
            var existingHoles = await _context.HoleScores
                .Where(hs => hs.MatchupId == matchupId)
                .ToListAsync();

            if (existingHoles.Any()) return true; // Already initialized

            // Standard 9-hole stroke index (1 = hardest, 9 = easiest)
            var defaultHoleHandicaps = new Dictionary<int, int>
            {
                { 1, 1 }, { 2, 5 }, { 3, 3 }, { 4, 7 }, { 5, 2 },
                { 6, 8 }, { 7, 4 }, { 8, 6 }, { 9, 9 }
            };

            // Standard 9-hole pars
            var defaultPars = new Dictionary<int, int>
            {
                { 1, 4 }, { 2, 3 }, { 3, 4 }, { 4, 5 }, { 5, 4 },
                { 6, 3 }, { 7, 4 }, { 8, 4 }, { 9, 5 }
            };

            for (int holeNumber = 1; holeNumber <= 9; holeNumber++)
            {
                var holeScore = new HoleScore
                {
                    Id = Guid.NewGuid(),
                    MatchupId = matchupId,
                    HoleNumber = holeNumber,
                    Par = defaultPars[holeNumber],
                    HoleHandicap = defaultHoleHandicaps[holeNumber],
                    PlayerAMatchPoints = 0,
                    PlayerBMatchPoints = 0
                };

                _context.HoleScores.Add(holeScore);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
