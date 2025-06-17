namespace GolfLeagueManager
{
    public class ScorecardSaveRequest
    {
        public Guid MatchupId { get; set; }
        public List<HoleScoreDto> HoleScores { get; set; } = new List<HoleScoreDto>();
        public int PlayerATotalScore { get; set; }
        public int PlayerBTotalScore { get; set; }
        // Absence scenario fields
        public bool PlayerAAbsent { get; set; }
        public bool PlayerBAbsent { get; set; }
        public bool PlayerAAbsentWithNotice { get; set; }
        public bool PlayerBAbsentWithNotice { get; set; }
    }

    public class HoleScoreDto
    {
        public int HoleNumber { get; set; }
        public int Par { get; set; }
        public int? PlayerAScore { get; set; }
        public int? PlayerBScore { get; set; }
    }

    public class ScorecardResponse
    {
        public Guid MatchupId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<HoleScore> HoleScores { get; set; } = new List<HoleScore>();
        
        // Match play results
        public int? PlayerAMatchPoints { get; set; }
        public int? PlayerBMatchPoints { get; set; }
        public int PlayerAHolePoints { get; set; }
        public int PlayerBHolePoints { get; set; }
        public bool PlayerAMatchWin { get; set; }
        public bool PlayerBMatchWin { get; set; }
        
        // Absence status
        public bool PlayerAAbsent { get; set; }
        public bool PlayerBAbsent { get; set; }
        public bool PlayerAAbsentWithNotice { get; set; }
        public bool PlayerBAbsentWithNotice { get; set; }
    }
}
