using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public class MatchPlayScoringService
    {
        private readonly AppDbContext _context;

        public MatchPlayScoringService(AppDbContext context)
        {
            _context = context;
        }
        public class MatchPlayResult
        {
            public int PlayerAHolePoints { get; set; }
            public int PlayerBHolePoints { get; set; }
            public int PlayerATotalPoints { get; set; }
            public int PlayerBTotalPoints { get; set; }
            public bool PlayerAMatchWin { get; set; }
            public bool PlayerBMatchWin { get; set; }
            public List<HoleResult> HoleResults { get; set; } = new List<HoleResult>();
        }

        public class HoleResult
        {
            public int HoleNumber { get; set; }
            public int PlayerANetScore { get; set; }
            public int PlayerBNetScore { get; set; }
            public int PlayerAPoints { get; set; }
            public int PlayerBPoints { get; set; }
            public string Winner { get; set; } = string.Empty; // "PlayerA", "PlayerB", or "Tie"
        }

        /// <summary>
        /// Calculate match play results based on hole scores and handicaps
        /// </summary>
        public MatchPlayResult CalculateMatchPlayResult(
            List<HoleScore> holeScores,
            decimal playerAHandicap,
            decimal playerBHandicap,
            int playerAGrossTotal = 0,
            int playerBGrossTotal = 0)
        {
            var result = new MatchPlayResult();
            var handicapDifference = Math.Abs(playerAHandicap - playerBHandicap);
            var playerAReceivesStrokes = playerAHandicap > playerBHandicap;

            // --- 9-hole stroke allocation fix ---
            // Only allocate strokes to the hardest holes within the 9 being played
            // Get the 9 holes being played (assume holeScores.Count == 9, holes 1-9 or 10-18)
            var holesInPlay = holeScores.Select(h => new { h.HoleNumber, h.HoleHandicap }).ToList();
            // Order by difficulty (Handicap index: 1 = hardest)
            var hardestHoles = holesInPlay.OrderBy(h => h.HoleHandicap).Take((int)Math.Round(handicapDifference)).Select(h => h.HoleNumber).ToHashSet();

            foreach (var hole in holeScores.OrderBy(h => h.HoleNumber))
            {
                if (!hole.PlayerAScore.HasValue || !hole.PlayerBScore.HasValue)
                    continue;

                var holeResult = CalculateHoleResult9Hole(
                    hole,
                    playerAReceivesStrokes,
                    hardestHoles
                );
                result.HoleResults.Add(holeResult);
                result.PlayerAHolePoints += holeResult.PlayerAPoints;
                result.PlayerBHolePoints += holeResult.PlayerBPoints;
            }

            int playerANetTotal = 0;
            int playerBNetTotal = 0;
            foreach (var holeResult in result.HoleResults)
            {
                playerANetTotal += holeResult.PlayerANetScore;
                playerBNetTotal += holeResult.PlayerBNetScore;
            }

            // Only award match points if there are actually completed holes
            if (result.HoleResults.Any())
            {
                if (playerANetTotal < playerBNetTotal)
                {
                    result.PlayerAMatchWin = true;
                    result.PlayerATotalPoints = result.PlayerAHolePoints + 2;
                    result.PlayerBTotalPoints = result.PlayerBHolePoints;
                }
                else if (playerBNetTotal < playerANetTotal)
                {
                    result.PlayerBMatchWin = true;
                    result.PlayerBTotalPoints = result.PlayerBHolePoints + 2;
                    result.PlayerATotalPoints = result.PlayerAHolePoints;
                }
                else
                {
                    result.PlayerAMatchWin = false;
                    result.PlayerBMatchWin = false;
                    result.PlayerATotalPoints = result.PlayerAHolePoints + 1;
                    result.PlayerBTotalPoints = result.PlayerBHolePoints + 1;
                }
            }
            else
            {
                // No completed holes - no match points awarded
                result.PlayerAMatchWin = false;
                result.PlayerBMatchWin = false;
                result.PlayerATotalPoints = 0;
                result.PlayerBTotalPoints = 0;
            }

            return result;
        }

        // New: 9-hole aware hole result calculation
        private HoleResult CalculateHoleResult9Hole(
            HoleScore hole,
            bool playerAReceivesStrokes,
            HashSet<int> hardestHoles
        )
        {
            var result = new HoleResult { HoleNumber = hole.HoleNumber };
            int strokesOnThisHole = hardestHoles.Contains(hole.HoleNumber) ? 1 : 0;
            if (playerAReceivesStrokes)
            {
                result.PlayerANetScore = hole.PlayerAScore!.Value - strokesOnThisHole;
                result.PlayerBNetScore = hole.PlayerBScore!.Value;
            }
            else
            {
                result.PlayerANetScore = hole.PlayerAScore!.Value;
                result.PlayerBNetScore = hole.PlayerBScore!.Value - strokesOnThisHole;
            }
            if (result.PlayerANetScore < result.PlayerBNetScore)
            {
                result.PlayerAPoints = 2;
                result.PlayerBPoints = 0;
                result.Winner = "PlayerA";
            }
            else if (result.PlayerBNetScore < result.PlayerANetScore)
            {
                result.PlayerAPoints = 0;
                result.PlayerBPoints = 2;
                result.Winner = "PlayerB";
            }
            else
            {
                result.PlayerAPoints = 1;
                result.PlayerBPoints = 1;
                result.Winner = "Tie";
            }
            return result;
        }

        private int CalculateStrokesForHole(decimal handicapDifference, int holeHandicap)
        {
            // Standard golf handicap allocation:
            // Holes are ranked 1-9 by difficulty (handicap index)
            // Player receives strokes on holes based on their handicap difference

            var totalStrokes = (int)Math.Round(handicapDifference);

            if (totalStrokes == 0) return 0;

            // First round: give strokes to holes 1-9
            if (holeHandicap <= Math.Min(totalStrokes, 9))
                return 1;

            // Second round: give additional strokes to holes 1-9 if handicap difference > 9
            if (totalStrokes > 9 && holeHandicap <= (totalStrokes - 9))
                return 2;

            return 0;
        }

        /// <summary>
        /// Calculate a player's handicap index based on their recent scores using World Handicap System (WHS)
        /// </summary>
        public decimal CalculateHandicapIndex(List<(int score, int courseRating, decimal slopeRating)> recentRounds)
        {
            if (!recentRounds.Any()) return 0;

            // Calculate score differentials for each round
            var differentials = recentRounds
                .Select(round => (decimal)(round.score - round.courseRating) * 113m / round.slopeRating)
                .OrderBy(d => d)
                .ToList();

            // Determine how many differentials to use based on WHS rules
            int differentialsToUse = GetDifferentialsCount(differentials.Count);
            
            if (differentialsToUse == 0) return 0;

            // Take the lowest differentials and average them
            var selectedDifferentials = differentials.Take(differentialsToUse);
            var averageDifferential = selectedDifferentials.Average();

            // Multiply by 0.96 to get handicap index
            var handicapIndex = averageDifferential * 0.96m;

            // Cap at reasonable limits (WHS allows up to 54.0)
            return Math.Max(0, Math.Min(36, Math.Round(handicapIndex, 1)));
        }

        /// <summary>
        /// Simplified handicap calculation for existing code compatibility
        /// </summary>
        public decimal CalculateHandicap(List<int> recentScores, int courseRating = 72, decimal slopeRating = 113)
        {
            var rounds = recentScores.Select(score => (score, courseRating, slopeRating)).ToList();
            return CalculateHandicapIndex(rounds);
        }

        /// <summary>
        /// Get number of differentials to use based on total available (WHS rules)
        /// </summary>
        private int GetDifferentialsCount(int totalDifferentials)
        {
            return totalDifferentials switch
            {
                >= 20 => 8,
                19 => 7,
                >= 17 => 6,
                >= 15 => 5,
                >= 12 => 4,
                >= 9 => 3,
                >= 6 => 2,
                >= 3 => 1,
                _ => 0
            };
        }

        /// <summary>
        /// Convert handicap index to course handicap for play
        /// </summary>
        public int GetCourseHandicap(decimal handicapIndex, decimal slopeRating = 113, int courseRating = 72, int par = 72)
        {
            var courseHandicap = handicapIndex * (slopeRating / 113m) + (courseRating - par);
            return (int)Math.Round(courseHandicap);
        }

        /// <summary>
        /// Get the handicap index for a specific hole from the database
        /// Course: Allentown Municipal Golf Course
        /// </summary>
        public async Task<int> GetHoleHandicapAsync(int holeNumber)
        {
            var courseHole = await _context.CourseHoles
                .FirstOrDefaultAsync(ch => ch.HoleNumber == holeNumber);

            return courseHole?.HandicapIndex ?? GetHoleHandicapFallback(holeNumber);
        }

        /// <summary>
        /// Fallback handicap values if database is not available
        /// </summary>
        private static int GetHoleHandicapFallback(int holeNumber)
        {
            var handicapMap = new Dictionary<int, int>
            {
                { 1, 3 },   { 2, 11 },  { 3, 1 },   { 4, 5 },   { 5, 17 },
                { 6, 7 },   { 7, 13 },  { 8, 9 },   { 9, 15 },  { 10, 12 },
                { 11, 4 },  { 12, 2 },  { 13, 6 },  { 14, 14 }, { 15, 10 },
                { 16, 8 },  { 17, 18 }, { 18, 16 }
            };

            return handicapMap.ContainsKey(holeNumber) ? handicapMap[holeNumber] : 18;
        }

        /// <summary>
        /// Calculate net score for a player on a specific hole
        /// </summary>
        public int CalculateNetScore(int grossScore, int playerHandicap, int opponentHandicap, int holeHandicap)
        {
            var strokesReceived = GetStrokesForHole(playerHandicap, opponentHandicap, holeHandicap);
            return grossScore - strokesReceived;
        }

        /// <summary>
        /// Get the number of strokes a player receives on a specific hole
        /// </summary>
        public int GetStrokesForHole(int playerHandicap, int opponentHandicap, int holeHandicap)
        {
            // Only the higher handicap player receives strokes
            if (playerHandicap <= opponentHandicap)
            {
                return 0; // No strokes for equal or lower handicap
            }

            var handicapDifference = playerHandicap - opponentHandicap;

            // Player gets strokes on holes based on hole handicap
            if (holeHandicap <= handicapDifference)
            {
                return 1; // Player gets 1 stroke on this hole
            }

            return 0; // No strokes on this hole
        }
    }
}
