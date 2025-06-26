using System;

namespace GolfLeagueManager.Models
{
    public class UpdateUserAdminStatusRequest
    {
        public Guid UserId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
