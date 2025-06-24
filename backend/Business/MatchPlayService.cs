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
            Console.WriteLine($"[DEBUG] CalculateMatchPlayResultsAsync called for matchup {matchupId}");
            
            // Get matchup with players and hole scores
            var matchup = await _context.Matchups
                .Include(m => m.PlayerA)
                .Include(m => m.PlayerB)
                .FirstOrDefaultAsync(m => m.Id == matchupId);

            if (matchup == null) 
            {
                Console.WriteLine($"[DEBUG] Matchup {matchupId} not found");
                return false;
            }

            Console.WriteLine($"[DEBUG] Matchup found: PlayerA={matchup.PlayerA?.FirstName} {matchup.PlayerA?.LastName}, PlayerB={matchup.PlayerB?.FirstName} {matchup.PlayerB?.LastName}");
            Console.WriteLine($"[DEBUG] Absence flags: PlayerAAbsent={matchup.PlayerAAbsent}, PlayerBAbsent={matchup.PlayerBAbsent}");

            // Handle absence scenarios first
            if (matchup.PlayerAAbsent || matchup.PlayerBAbsent)
            {
                Console.WriteLine("[DEBUG] Calling CalculateAbsenceScenarioAsync");
                return await CalculateAbsenceScenarioAsync(matchup);
            }

            Console.WriteLine("[DEBUG] No absence scenario detected, proceeding with normal match play calculation");
            
            var holeScores = await _context.HoleScores
                .Where(hs => hs.MatchupId == matchupId)
                .OrderBy(hs => hs.HoleNumber)
                .ToListAsync();

            if (!holeScores.Any()) 
            {
                Console.WriteLine("[DEBUG] No hole scores found");
                return false;
            }

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

            // Special circumstance points logic (applies to both normal and absence scenarios)
            var week = await _context.Weeks.FirstOrDefaultAsync(w => w.Id == matchup.WeekId);
            if (week != null && week.SpecialPointsAwarded.HasValue)
            {
                int special = week.SpecialPointsAwarded.Value;
                matchup.PlayerAPoints = matchup.PlayerAAbsent ? special / 2 : special;
                matchup.PlayerBPoints = matchup.PlayerBAbsent ? special / 2 : special;
                // Optionally, set hole points to 0 for special weeks
                matchup.PlayerAHolePoints = 0;
                matchup.PlayerBHolePoints = 0;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Calculate points for absence scenarios
        /// </summary>
        private async Task<bool> CalculateAbsenceScenarioAsync(Matchup matchup)
        {
            Console.WriteLine($"[DEBUG] CalculateAbsenceScenarioAsync called for matchup {matchup.Id}");
            Console.WriteLine($"[DEBUG] PlayerAAbsent: {matchup.PlayerAAbsent}, PlayerBAbsent: {matchup.PlayerBAbsent}");
            Console.WriteLine($"[DEBUG] PlayerAAbsentWithNotice: {matchup.PlayerAAbsentWithNotice}, PlayerBAbsentWithNotice: {matchup.PlayerBAbsentWithNotice}");

            if (matchup.PlayerAAbsent && matchup.PlayerBAbsent)
            {
                Console.WriteLine("[DEBUG] Both players absent scenario");
                // Both absent - distribute points based on notice
                if (matchup.PlayerAAbsentWithNotice && matchup.PlayerBAbsentWithNotice)
                {
                    // Both gave notice - split points evenly
                    matchup.PlayerAPoints = 4;
                    matchup.PlayerBPoints = 4;
                    Console.WriteLine("[DEBUG] Both gave notice: A=10, B=10");
                }
                else if (matchup.PlayerAAbsentWithNotice)
                {
                    // Only A gave notice
                    matchup.PlayerAPoints = 4;
                    matchup.PlayerBPoints = 0;
                    Console.WriteLine("[DEBUG] Only A gave notice: A=4, B=0");
                    // Remaining 16 points are forfeited
                }
                else if (matchup.PlayerBAbsentWithNotice)
                {
                    // Only B gave notice
                    matchup.PlayerAPoints = 0;
                    matchup.PlayerBPoints = 4;
                    Console.WriteLine("[DEBUG] Only B gave notice: A=0, B=4");
                    // Remaining 16 points are forfeited
                }
                else
                {
                    // Neither gave notice
                    matchup.PlayerAPoints = 0;
                    matchup.PlayerBPoints = 0;
                    Console.WriteLine("[DEBUG] Neither gave notice: A=0, B=0");
                    // All 20 points are forfeited
                }
                matchup.PlayerAHolePoints = 0;
                matchup.PlayerBHolePoints = 0;
                matchup.PlayerAMatchWin = false;
                matchup.PlayerBMatchWin = false;
            }
            else if (matchup.PlayerAAbsent)
            {
                Console.WriteLine("[DEBUG] Player A absent, Player B present");
                // Player A absent, Player B present
                var playerAPoints = matchup.PlayerAAbsentWithNotice ? 4 : 0;
                var playerBHandicap = await GetPlayerHandicapAsync(matchup.PlayerBId);
                var playerBPoints = await CalculateNoOpponentScoringAsync(matchup, matchup.PlayerBId, playerBHandicap);
                
                Console.WriteLine($"[DEBUG] Player A points: {playerAPoints}, Player B points: {playerBPoints}");
                
                matchup.PlayerAPoints = playerAPoints;
                matchup.PlayerBPoints = playerBPoints;
                matchup.PlayerAHolePoints = 0;
                matchup.PlayerBHolePoints = Math.Max(0, playerBPoints - 2); // Subtract match bonus to get hole points
                matchup.PlayerAMatchWin = false;
                matchup.PlayerBMatchWin = playerBPoints > playerAPoints;
                
                Console.WriteLine($"[DEBUG] Final Player A: Total={matchup.PlayerAPoints}, Hole={matchup.PlayerAHolePoints}, Win={matchup.PlayerAMatchWin}");
                Console.WriteLine($"[DEBUG] Final Player B: Total={matchup.PlayerBPoints}, Hole={matchup.PlayerBHolePoints}, Win={matchup.PlayerBMatchWin}");
            }
            else if (matchup.PlayerBAbsent)
            {
                Console.WriteLine("[DEBUG] Player B absent, Player A present");
                // Player B absent, Player A present
                var playerBPoints = matchup.PlayerBAbsentWithNotice ? 4 : 0;
                var playerAHandicap = await GetPlayerHandicapAsync(matchup.PlayerAId);
                var playerAPoints = await CalculateNoOpponentScoringAsync(matchup, matchup.PlayerAId, playerAHandicap);
                
                Console.WriteLine($"[DEBUG] Player A points: {playerAPoints}, Player B points: {playerBPoints}");
                
                matchup.PlayerAPoints = playerAPoints;
                matchup.PlayerBPoints = playerBPoints;
                matchup.PlayerAHolePoints = Math.Max(0, playerAPoints - 2); // Subtract match bonus to get hole points
                matchup.PlayerBHolePoints = 0;
                matchup.PlayerAMatchWin = playerAPoints > playerBPoints;
                matchup.PlayerBMatchWin = false;
                
                Console.WriteLine($"[DEBUG] Final Player A: Total={matchup.PlayerAPoints}, Hole={matchup.PlayerAHolePoints}, Win={matchup.PlayerAMatchWin}");
                Console.WriteLine($"[DEBUG] Final Player B: Total={matchup.PlayerBPoints}, Hole={matchup.PlayerBHolePoints}, Win={matchup.PlayerBMatchWin}");
            }

            Console.WriteLine($"[DEBUG] Final points after absence calculation: PlayerA={matchup.PlayerAPoints}, PlayerB={matchup.PlayerBPoints}");

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Calculate points when player has no opponent (absence scenario)
        /// Rules: Present player gets 16 points if they beat their average by a whole number, otherwise 8 points
        /// No additional match win bonus is awarded in absence scenarios
        /// </summary>
        private async Task<int> CalculateNoOpponentScoringAsync(Matchup matchup, Guid playerId, double handicap)
        {
            Console.WriteLine($"[DEBUG] CalculateNoOpponentScoringAsync called for player {playerId}");
            
            // Get the player's current average score
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
            {
                Console.WriteLine($"[DEBUG] Player {playerId} not found, returning default 8 points");
                return 8; // Default fallback: 8 points (no match bonus in absence scenario)
            }

            Console.WriteLine($"[DEBUG] Player found: {player.FirstName} {player.LastName}, CurrentAverageScore: {player.CurrentAverageScore}");

            // Get hole scores for this matchup to calculate total score
            var holeScores = await _context.HoleScores
                .Where(hs => hs.MatchupId == matchup.Id)
                .OrderBy(hs => hs.HoleNumber)
                .ToListAsync();

            Console.WriteLine($"[DEBUG] Found {holeScores.Count} hole scores for matchup {matchup.Id}");

            if (!holeScores.Any())
            {
                Console.WriteLine("[DEBUG] No hole scores available, returning 8 points");
                // No hole scores available, award minimum points: 8 points (no match bonus in absence scenario)
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
                    Console.WriteLine($"[DEBUG] Hole {holeScore.HoleNumber}: Score = {actualScore.Value}");
                }
            }

            Console.WriteLine($"[DEBUG] Total score: {totalScore}, HasValidScore: {hasValidScore}");

            if (!hasValidScore)
            {
                Console.WriteLine("[DEBUG] No valid scores found, returning 8 points");
                // No valid scores found, award minimum points: 8 points (no match bonus in absence scenario)
                return 8;
            }

            // Check if player beat their average by a whole number
            // Example: If average is 43.99, they need to shoot 42 or better
            var averageScore = player.CurrentAverageScore;
            var requiredScore = Math.Floor(averageScore); // This gives us the whole number threshold
            
            Console.WriteLine($"[DEBUG] Player average: {averageScore}, Required score to get 16 points: {requiredScore}, Actual score: {totalScore}");
            
            int totalPoints;
            if (totalScore < requiredScore)
            {
                Console.WriteLine("[DEBUG] Player beat their average by a whole number - awarding 16 points");
                totalPoints = 16;
            }
            else
            {
                Console.WriteLine("[DEBUG] Player did not beat their average by a whole number - awarding 8 points");
                totalPoints = 8;
            }
            
            // In absence scenario, no match win bonus is awarded
            Console.WriteLine($"[DEBUG] Final points for absence scenario: {totalPoints}");
            
            return totalPoints;
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
