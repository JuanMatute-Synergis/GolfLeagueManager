using System;
using System.ComponentModel.DataAnnotations;

namespace GolfLeagueManager.Models
{
    public class LinkUserToPlayerRequest
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public Guid PlayerId { get; set; }
    }
}
