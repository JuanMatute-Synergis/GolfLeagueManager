using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GolfLeagueManager.Models
{
    public class LeagueRules
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid SeasonId { get; set; }
        
        [ForeignKey("SeasonId")]
        public Season? Season { get; set; }
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        public string? CreatedBy { get; set; }
        
        public string? UpdatedBy { get; set; }
    }
    
    public class UpdateRulesRequest
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }
}
