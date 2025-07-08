using System;
using System.Linq;
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

        // GET: api/UserAdmin/players-with-account-status
        [HttpGet("players-with-account-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPlayersWithAccountStatus()
        {
            var players = await _context.Players.ToListAsync();
            var users = await _context.Users.ToListAsync();
            
            // Debug logging
            Console.WriteLine($"Found {players.Count} players and {users.Count} users");
            foreach (var user in users)
            {
                Console.WriteLine($"User: {user.Username}, PlayerId: {user.PlayerId}");
            }
            
            var result = players.Select(p => {
                var user = users.FirstOrDefault(u => u.PlayerId == p.Id);
                Console.WriteLine($"Player: {p.FirstName} {p.LastName} (ID: {p.Id}), HasAccount: {user != null}");
                return new Models.PlayerAccountStatusDto
                {
                    PlayerId = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    ImageUrl = p.ImageUrl,
                    Email = p.Email,
                    Phone = p.Phone, // Add Phone field
                    HasUserAccount = user != null,
                    Username = user?.Username,
                    UserId = user?.Id,
                    IsAdmin = user?.IsAdmin
                };
            }).ToList();
            return Ok(result);
        }

        // POST: api/UserAdmin/create-for-player
        [HttpPost("create-for-player")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUserForPlayer([FromBody] CreateUserForPlayerRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username already exists");
            if (await _context.Users.AnyAsync(u => u.PlayerId == request.PlayerId))
                return BadRequest("A user account already exists for this player");
            var player = await _context.Players.FindAsync(request.PlayerId);
            if (player == null)
                return NotFound("Player not found");
            var user = new User
            {
                Username = request.Username,
                PasswordHash = AuthController.HashPassword(request.Password),
                PlayerId = request.PlayerId,
                IsAdmin = request.IsAdmin
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { user.Username, user.PlayerId });
        }

        // POST: api/UserAdmin/reset-password
        [HttpPost("reset-password")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetUserPassword([FromBody] ResetUserPasswordRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return NotFound("User not found");
            user.PasswordHash = AuthController.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();
            return Ok(new { user.Username, message = "Password reset successfully" });
        }

        // GET: api/UserAdmin/debug-users
        [HttpGet("debug-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersDebug()
        {
            var users = await _context.Users.Select(u => new 
            {
                u.Id,
                u.Username,
                u.PlayerId,
                u.IsAdmin
            }).ToListAsync();
            return Ok(users);
        }

        // GET: api/UserAdmin/debug-players  
        [HttpGet("debug-players")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPlayersDebug()
        {
            var players = await _context.Players.Select(p => new 
            {
                p.Id,
                p.FirstName,
                p.LastName,
                p.Email
            }).ToListAsync();
            return Ok(players);
        }

        // POST: api/UserAdmin/link-user-to-player
        [HttpPost("link-user-to-player")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LinkUserToPlayer([FromBody] LinkUserToPlayerRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return NotFound("User not found");

            var player = await _context.Players.FindAsync(request.PlayerId);
            if (player == null)
                return NotFound("Player not found");

            user.PlayerId = request.PlayerId;
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "User linked to player successfully" });
        }

        // POST: api/UserAdmin/update-account
        [HttpPost("update-account")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserAccount([FromBody] UpdateUserAccountRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return NotFound("User not found");

            // Check if username is already taken by another user
            if (await _context.Users.AnyAsync(u => u.Username == request.Username && u.Id != request.UserId))
                return BadRequest("Username already exists");

            user.Username = request.Username;
            user.IsAdmin = request.IsAdmin;
            
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "User account updated successfully" });
        }
    }
}
