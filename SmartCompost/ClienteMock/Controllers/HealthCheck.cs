using Microsoft.AspNetCore.Mvc;

namespace MockSmartcompost.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthCheck : ControllerBase
    {
        [HttpGet]
        public IActionResult Alive()
        {
            return Ok();
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
}
