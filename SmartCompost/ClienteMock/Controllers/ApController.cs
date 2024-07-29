using Microsoft.AspNetCore.Mvc;
using MockSmartcompost.Dto;
using System.Collections.Concurrent;

namespace MockSmartcompost.Controllers
{
    [ApiController]
    [Route("api/ap")]
    public class ApController : ControllerBase
    {
        private static DateTime ultimoMensajeRecibido = DateTime.MinValue;
        private static ConcurrentDictionary<string, List<ApMedicionesDto>> mensajesPorNodo = new ConcurrentDictionary<string, List<ApMedicionesDto>>();

        [HttpPost("{serialNumber}/measurements")]
        public IActionResult PostMeasurements([FromRoute] string serialNumber, [FromBody] ApMedicionesDto medicion)
        {
            //TERMINAR
            return Ok();
        }

        [HttpGet("last-measurement")]
        public IActionResult UltimaMedicion()
        {

            return Ok(ultimoMensajeRecibido.ToString());
        }

        [HttpGet("measurements")]
        public IActionResult GetAll()
        {
            return Ok(mensajesPorNodo.Values.ToList());
        }
    }
}
