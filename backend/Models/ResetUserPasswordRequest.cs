using System;
using System.ComponentModel.DataAnnotations;

namespace GolfLeagueManager.Models
{
    public class ResetUserPasswordRequest
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}
