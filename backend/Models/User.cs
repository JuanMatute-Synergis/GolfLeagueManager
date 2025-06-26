using System;
using System.ComponentModel.DataAnnotations;

namespace GolfLeagueManager.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public required string Username { get; set; }
        [Required]
        public required string PasswordHash { get; set; }
        public bool IsAdmin { get; set; } = false;

        // Link to Player profile (optional)
        public Guid? PlayerId { get; set; }
        public Player? Player { get; set; }
        // Optionally add roles, email, etc.
    }
}
