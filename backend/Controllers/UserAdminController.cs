using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GolfLeagueManager.Models;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserAdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserAdminController(AppDbContext context)
        {
            _context = context;
        }

        // Simple endpoint to create a user (for initial setup, remove or secure in production)
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] LoginRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username already exists");
            var user = new User
            {
                Username = request.Username,
                PasswordHash = AuthController.HashPassword(request.Password)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { user.Username });
        }
    }
}
