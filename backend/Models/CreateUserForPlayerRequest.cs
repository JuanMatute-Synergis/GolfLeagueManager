using System;
using System.ComponentModel.DataAnnotations;

namespace GolfLeagueManager.Models
{
    public class CreateUserForPlayerRequest
    {
        [Required]
        public Guid PlayerId { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;
    }
}
