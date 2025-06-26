using System;
using System.ComponentModel.DataAnnotations;

namespace GolfLeagueManager.Models
{
    public class UpdateUserAccountRequest
    {
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        public bool IsAdmin { get; set; }
    }
}
