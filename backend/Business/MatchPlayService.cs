using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public class MatchPlayService
    {
        private readonly AppDbContext _context;

        public MatchPlayService(AppDbContext context)
        {
            _context = context;
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

            // Get handicaps for both players from their flight assignments
            var playerAHandicap = await GetPlayerHandicapAsync(matchup.PlayerAId);
            var playerBHandicap = await GetPlayerHandicapAsync(matchup.PlayerBId);

            // Calculate handicap difference and stroke allocation
            var handicapDifference = Math.Abs(playerAHandicap - playerBHandicap);
            var strokesForPlayerA = playerAHandicap > playerBHandicap ? (int)Math.Round(handicapDifference) : 0;
            var strokesForPlayerB = playerBHandicap > playerAHandicap ? (int)Math.Round(handicapDifference) : 0;

            // Calculate match play points for each hole
            int playerAHolePoints = 0;
            int playerBHolePoints = 0;

            foreach (var holeScore in holeScores)
            {
                if (!holeScore.PlayerAScore.HasValue || !holeScore.PlayerBScore.HasValue)
                    continue;

                // Calculate net scores (apply handicap strokes to hardest holes first)
                var playerANetScore = holeScore.PlayerAScore.Value;
                var playerBNetScore = holeScore.PlayerBScore.Value;

                // Apply handicap strokes based on hole difficulty (stroke index)
                if (strokesForPlayerA > 0 && holeScore.HoleHandicap <= strokesForPlayerA)
                {
                    playerANetScore -= 1;
                }
                if (strokesForPlayerB > 0 && holeScore.HoleHandicap <= strokesForPlayerB)
                {
                    playerBNetScore -= 1;
                }

                // Determine hole winner and assign match play points
                if (playerANetScore < playerBNetScore)
                {
                    // Player A wins hole
                    holeScore.PlayerAMatchPoints = 2;
                    holeScore.PlayerBMatchPoints = 0;
                    playerAHolePoints += 2;
                }
                else if (playerBNetScore < playerANetScore)
                {
                    // Player B wins hole
                    holeScore.PlayerAMatchPoints = 0;
                    holeScore.PlayerBMatchPoints = 2;
                    playerBHolePoints += 2;
                }
                else
                {
                    // Tie
                    holeScore.PlayerAMatchPoints = 1;
                    holeScore.PlayerBMatchPoints = 1;
                    playerAHolePoints += 1;
                    playerBHolePoints += 1;
                }
            }

            // Determine overall match winner and assign bonus points
            bool playerAWinsMatch = playerAHolePoints > playerBHolePoints;
            bool playerBWinsMatch = playerBHolePoints > playerAHolePoints;

            // Update matchup with results
            matchup.PlayerAHolePoints = playerAHolePoints;
            matchup.PlayerBHolePoints = playerBHolePoints;
            matchup.PlayerAMatchWin = playerAWinsMatch;
            matchup.PlayerBMatchWin = playerBWinsMatch;

            // Calculate total points (hole points + match bonus)
            matchup.PlayerAPoints = playerAHolePoints + (playerAWinsMatch ? 2 : 0);
            matchup.PlayerBPoints = playerBHolePoints + (playerBWinsMatch ? 2 : 0);

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
                // Both absent - no points awarded
                matchup.PlayerAPoints = 0;
                matchup.PlayerBPoints = 0;
                matchup.PlayerAHolePoints = 0;
                matchup.PlayerBHolePoints = 0;
                matchup.PlayerAMatchWin = false;
                matchup.PlayerBMatchWin = false;
            }
            else if (matchup.PlayerAAbsent)
            {
                // Player A absent, Player B present
                if (matchup.PlayerAAbsentWithNotice)
                {
                    matchup.PlayerAPoints = 4; // Unable to play with notice
                }
                else
                {
                    matchup.PlayerAPoints = 0; // No notice
                }

                // Player B gets points based on performance vs handicap
                var playerBHandicap = await GetPlayerHandicapAsync(matchup.PlayerBId);
                await CalculateNoOpponentScoringAsync(matchup, matchup.PlayerBId, playerBHandicap, false);
            }
            else if (matchup.PlayerBAbsent)
            {
                // Player B absent, Player A present
                if (matchup.PlayerBAbsentWithNotice)
                {
                    matchup.PlayerBPoints = 4; // Unable to play with notice
                }
                else
                {
                    matchup.PlayerBPoints = 0; // No notice
                }

                // Player A gets points based on performance vs handicap
                var playerAHandicap = await GetPlayerHandicapAsync(matchup.PlayerAId);
                await CalculateNoOpponentScoringAsync(matchup, matchup.PlayerAId, playerAHandicap, true);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Calculate points when player has no opponent (beat handicap scenario)
        /// </summary>
        private async Task<bool> CalculateNoOpponentScoringAsync(Matchup matchup, Guid playerId, double handicap, bool isPlayerA)
        {
            var playerScore = isPlayerA ? matchup.PlayerAScore : matchup.PlayerBScore;
            
            if (!playerScore.HasValue)
            {
                // No score recorded, award 0 points
                if (isPlayerA)
                {
                    matchup.PlayerAPoints = 0;
                    matchup.PlayerAHolePoints = 0;
                    matchup.PlayerAMatchWin = false;
                }
                else
                {
                    matchup.PlayerBPoints = 0;
                    matchup.PlayerBHolePoints = 0;
                    matchup.PlayerBMatchWin = false;
                }
                return true;
            }

            // Calculate expected score based on handicap (assuming par 36 for 9 holes)
            const int standardPar = 36;
            var expectedScore = standardPar + (int)Math.Round(handicap);
            
            // Determine points based on performance vs handicap
            int pointsAwarded;
            if (playerScore.Value < expectedScore)
            {
                // Beat handicap
                pointsAwarded = 16;
            }
            else if (playerScore.Value == expectedScore)
            {
                // Tied handicap
                pointsAwarded = 8;
            }
            else
            {
                // Didn't beat handicap
                pointsAwarded = 8;
            }

            if (isPlayerA)
            {
                matchup.PlayerAPoints = pointsAwarded;
                matchup.PlayerAHolePoints = pointsAwarded - 2; // Simulate hole points
                matchup.PlayerAMatchWin = pointsAwarded > 10; // Simulate match win
            }
            else
            {
                matchup.PlayerBPoints = pointsAwarded;
                matchup.PlayerBHolePoints = pointsAwarded - 2; // Simulate hole points
                matchup.PlayerBMatchWin = pointsAwarded > 10; // Simulate match win
            }

            return true;
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
