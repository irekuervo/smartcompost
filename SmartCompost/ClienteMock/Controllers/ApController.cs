using Microsoft.AspNetCore.Mvc;
using MockSmartcompost.Dto;
using MockSmartcompost.Utils;
using System.Collections.Concurrent;
using System.Text.Json;

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
            if (medicion == null)
                return BadRequest("Medicion is null");

            AppLogger.Log(JsonSerializer.Serialize(medicion));

            if (mensajesPorNodo.ContainsKey(serialNumber) == false)
                mensajesPorNodo.TryAdd(serialNumber, new List<ApMedicionesDto>());

            mensajesPorNodo[serialNumber].Add(medicion);

            ultimoMensajeRecibido = DateTime.Now;

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
