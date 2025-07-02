using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace GolfLeagueManager
{
    /// <summary>
    /// Represents league-specific scoring and calculation settings
    /// </summary>
    public class LeagueSettings
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid SeasonId { get; set; }
        
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
        /// Points for tie match
        /// </summary>
        public int MatchTiePoints { get; set; } = 1;
        
        // Custom Settings
        /// <summary>
        /// Whether to use session-based handicaps
        /// </summary>
        public bool UseSessionHandicaps { get; set; } = true;
        
        /// <summary>
        /// Whether to allow handicap updates during the season
        /// </summary>
        public bool AllowHandicapUpdates { get; set; } = true;
        
        /// <summary>
        /// Custom rules or notes for this league
        /// </summary>
        public string? CustomRules { get; set; }
        
        // Audit fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation properties
        [JsonIgnore]
        public Season Season { get; set; } = null!;
    }
}
