using Microsoft.AspNetCore.Mvc;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestAuthController : ControllerBase
    {
        [HttpGet("protected")]
        public IActionResult Protected()
        {
            return Ok(new { message = "You are authenticated!", user = User.Identity?.Name });
        }
    }
}
