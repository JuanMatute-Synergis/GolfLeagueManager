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
            decimal playerBHandicap)
        {
            var result = new MatchPlayResult();
            
            // Calculate net handicap difference
            var handicapDifference = Math.Abs(playerAHandicap - playerBHandicap);
            var playerAReceivesStrokes = playerAHandicap > playerBHandicap;
            
            foreach (var hole in holeScores.OrderBy(h => h.HoleNumber))
            {
                if (!hole.PlayerAScore.HasValue || !hole.PlayerBScore.HasValue)
                    continue;

                var holeResult = CalculateHoleResult(
                    hole, 
                    handicapDifference, 
                    playerAReceivesStrokes);
                
                result.HoleResults.Add(holeResult);
                result.PlayerAHolePoints += holeResult.PlayerAPoints;
                result.PlayerBHolePoints += holeResult.PlayerBPoints;
            }

            // Determine match winner based on hole points and award 2-point bonus
            if (result.PlayerAHolePoints > result.PlayerBHolePoints)
            {
                result.PlayerAMatchWin = true;
                result.PlayerATotalPoints = result.PlayerAHolePoints + 2; // 2-point match bonus
                result.PlayerBTotalPoints = result.PlayerBHolePoints;
            }
            else if (result.PlayerBHolePoints > result.PlayerAHolePoints)
            {
                result.PlayerBMatchWin = true;
                result.PlayerBTotalPoints = result.PlayerBHolePoints + 2; // 2-point match bonus
                result.PlayerATotalPoints = result.PlayerAHolePoints;
            }
            else
            {
                // Tie - no match bonus
                result.PlayerATotalPoints = result.PlayerAHolePoints;
                result.PlayerBTotalPoints = result.PlayerBHolePoints;
            }

            return result;
        }

        private HoleResult CalculateHoleResult(
            HoleScore hole, 
            decimal handicapDifference, 
            bool playerAReceivesStrokes)
        {
            var result = new HoleResult
            {
                HoleNumber = hole.HoleNumber
            };

            // Calculate net scores with handicap strokes
            var strokesOnThisHole = CalculateStrokesForHole(handicapDifference, hole.HoleHandicap);
            
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

            // Award points based on net scores
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
        /// Calculate a player's handicap based on their recent scores
        /// This is a simplified handicap calculation - in real golf, it's more complex
        /// </summary>
        public decimal CalculateHandicap(List<int> recentScores, int courseRating = 72, decimal slopeRating = 113)
        {
            if (!recentScores.Any()) return 0;

            // Take the best 8 scores from the last 20 rounds (simplified)
            var bestScores = recentScores
                .OrderBy(s => s)
                .Take(Math.Min(8, recentScores.Count))
                .ToList();

            if (!bestScores.Any()) return 0;

            // Calculate differential for each score
            var differentials = bestScores
                .Select(score => (score - courseRating) * 113 / slopeRating)
                .ToList();

            // Average the differentials and multiply by 0.96
            var handicap = differentials.Average() * 0.96m;

            // Cap at reasonable limits
            return Math.Max(0, Math.Min(36, Math.Round(handicap, 1)));
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
    }
}
