using Microsoft.AspNetCore.Mvc;

namespace MockSmartcompost.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthCheck : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
}
