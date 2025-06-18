using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GolfLeagueManager
{
    public class Course
    {
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Location { get; set; } = string.Empty;
        
        public int TotalPar { get; set; }
        
        public int TotalYardage { get; set; }
        
        public decimal SlopeRating { get; set; } = 113;
        
        public decimal CourseRating { get; set; } = 72;
        
        // Navigation property
        public List<CourseHole> CourseHoles { get; set; } = new List<CourseHole>();
    }
    
    public class CourseHole
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid CourseId { get; set; }
        
        [Required]
        public int HoleNumber { get; set; }
        
        [Required]
        public int Par { get; set; }
        
        public int Yardage { get; set; }
        
        [Required]
        public int HandicapIndex { get; set; } // 1-18, where 1 is the hardest hole
        
        // Navigation property
        [JsonIgnore]
        public Course? Course { get; set; }
    }
}
