using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using GolfLeagueManager.Models;
using System.Security.Cryptography;
using GolfLeagueManager.Helpers;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized("Invalid username or password");

            var token = GenerateJwtToken(user);
            
            // Set HttpOnly cookie for JWT
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Only over HTTPS in production
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7), // Match JWT expiration
                Path = "/"
            };
            
            Response.Cookies.Append("golf_jwt_token", token, cookieOptions);
            
            // Return success without the token in the response body
            return Ok(new LoginResponse { Token = "", Username = user.Username });
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Clear the JWT cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1), // Expire immediately
                Path = "/"
            };
            
            Response.Cookies.Append("golf_jwt_token", "", cookieOptions);
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("status")]
        public IActionResult GetAuthStatus()
        {
            // This endpoint requires authentication, so if we reach here, the user is authenticated
            var username = User.Identity?.Name;
            return Ok(new { authenticated = true, username = username });
        }

        // Helper: Hash password
        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // Helper: Verify password
        public static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        // Helper: Generate JWT
        private string GenerateJwtToken(User user)
        {
            var privateKeyPath = _config["Jwt:PrivateKeyPath"] ?? "jwt_private_key.pem";
            var rsaKey = RsaKeyHelper.GetPrivateKey(privateKeyPath);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
