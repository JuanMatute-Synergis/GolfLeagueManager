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
        SimpleAverage = 1
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
