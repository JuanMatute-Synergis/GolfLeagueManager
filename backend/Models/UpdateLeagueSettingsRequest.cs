using System.ComponentModel.DataAnnotations;

namespace GolfLeagueManager
{
    /// <summary>
    /// DTO for updating league settings without requiring ID
    /// </summary>
    public class UpdateLeagueSettingsRequest
    {
        // Handicap Calculation Settings
        public HandicapCalculationMethod HandicapMethod { get; set; } = HandicapCalculationMethod.WorldHandicapSystem;

        /// <summary>
        /// Course par for simple average handicap calculation (typically 36 for 9-hole)
        /// </summary>
        public int CoursePar { get; set; } = 36;

        /// <summary>
        /// Course rating for World Handicap System
        /// </summary>
        public decimal CourseRating { get; set; } = 35.0m;

        /// <summary>
        /// Slope rating for World Handicap System  
        /// </summary>
        public decimal SlopeRating { get; set; } = 113m;

        /// <summary>
        /// Maximum number of recent rounds to consider for WHS (default 20)
        /// </summary>
        public int MaxRoundsForHandicap { get; set; } = 20;

        // Scoring Method Settings
        public ScoringMethod ScoringMethod { get; set; } = ScoringMethod.MatchPlay;

        // Points System Settings
        public PointsSystem PointsSystem { get; set; } = PointsSystem.HolePointsWithMatchBonus;

        /// <summary>
        /// Points awarded for winning a hole in match play
        /// </summary>
        public int HoleWinPoints { get; set; } = 2;

        /// <summary>
        /// Points awarded for halving a hole in match play
        /// </summary>
        public int HoleHalvePoints { get; set; } = 1;

        /// <summary>
        /// Bonus points for winning the overall match
        /// </summary>
        public int MatchWinBonus { get; set; } = 2;

        /// <summary>
        /// Points for each player when match ends in a tie
        /// </summary>
        public int MatchTiePoints { get; set; } = 1;

        /// <summary>
        /// Whether to use session-specific handicaps
        /// </summary>
        public bool UseSessionHandicaps { get; set; } = false;

        /// <summary>
        /// Whether to automatically update handicaps after each round
        /// </summary>
        public bool AllowHandicapUpdates { get; set; } = true;

        /// <summary>
        /// Custom league rules or notes
        /// </summary>
        public string? CustomRules { get; set; }
    }
}
