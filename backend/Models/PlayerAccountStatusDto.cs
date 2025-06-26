using System;

namespace GolfLeagueManager.Models
{
    public class PlayerAccountStatusDto
    {
        public Guid PlayerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool HasUserAccount { get; set; }
        public string? Username { get; set; }
        public Guid? UserId { get; set; } // Add UserId for password reset functionality
        public bool? IsAdmin { get; set; } // Admin status for linked accounts
    }
}
