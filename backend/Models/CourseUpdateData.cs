using System.ComponentModel.DataAnnotations;

namespace GolfLeagueManager
{
    public class CourseUpdateData
    {
        public string? Location { get; set; }
        public decimal? CourseRating { get; set; }
        public decimal? SlopeRating { get; set; }
        public int? TotalYardage { get; set; }
        public List<HoleUpdateData>? Holes { get; set; }
    }

    public class HoleUpdateData
    {
        [Required]
        public int HoleNumber { get; set; }
        public int? Par { get; set; }
        public int? HandicapIndex { get; set; }
        public int? Yardage { get; set; }
    }
}
