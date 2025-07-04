namespace GolfLeagueManager
{
    public enum HandicapCalculationMethod
    {
        /// <summary>
        /// World Handicap System - Best 8 of last 20 scores with differentials
        /// </summary>
        WorldHandicapSystem = 0,

        /// <summary>
        /// Simple overall average - (Average Score - Course Par)
        /// Based on analysis from FINAL_HANDICAP_ANALYSIS.md
        /// </summary>
        SimpleAverage = 1,

        /// <summary>
        /// Legacy Lookup Table System - Maps average scores to specific handicaps using predefined table
        /// Based on the other system's handicap calculation method
        /// </summary>
        LegacyLookupTable = 2
    }

    public enum AverageCalculationMethod
    {
        /// <summary>
        /// Current system - Simple arithmetic average with initial baseline
        /// Formula: (Initial Average + Sum of actual scores) / (1 + Number of actual rounds played)
        /// </summary>
        SimpleAverage = 0,

        /// <summary>
        /// Legacy Weighted Average System - Uses weighted average where initial average counts as multiple rounds
        /// Formula: (initial_average × initial_weight + sum_of_scores) / (initial_weight + number_of_scores)
        /// Initial weight typically 4, weeks 1-3 don't count toward handicap
        /// </summary>
        LegacyWeightedAverage = 1
    }

    public enum ScoringMethod
    {
        /// <summary>
        /// Match play scoring with hole-by-hole competition
        /// </summary>
        MatchPlay = 0,

        /// <summary>
        /// Stroke play scoring based on total strokes
        /// </summary>
        StrokePlay = 1
    }

    public enum PointsSystem
    {
        /// <summary>
        /// Current system: hole points + 2 point match bonus
        /// </summary>
        HolePointsWithMatchBonus = 0,

        /// <summary>
        /// Points based only on final score comparison
        /// </summary>
        ScoreBasedPoints = 1,

        /// <summary>
        /// Custom points allocation system
        /// </summary>
        Custom = 2
    }
}
