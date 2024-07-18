using Microsoft.AspNetCore.Mvc;
using MockSmartcompost.Dto;
using System.Text.Json;

namespace MockSmartcompost.Controllers
{
    [ApiController]
    [Route("api/nodes")]
    public class MedicionController : ControllerBase
    {
        [HttpPost("{nodeId:int}/measurements")]
        public IActionResult PostMeasurement(int nodeId, [FromBody] MensajeMediciones medicion)
        {
            if (medicion == null)
                return BadRequest("Medicion is null");

            Console.WriteLine(JsonSerializer.Serialize(medicion));

            return Ok("OK");
        }
    }
}
