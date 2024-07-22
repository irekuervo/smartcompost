using Microsoft.AspNetCore.Mvc;
using MockSmartcompost.Dto;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MockSmartcompost.Controllers
{
    [ApiController]
    [Route("api/nodes")]
    public class MedicionController : ControllerBase
    {
        private static DateTime ultimoMensajeRecibido = DateTime.MinValue;
        private static ConcurrentDictionary<int, List<MensajeMediciones>> mensajesPorNodo = new ConcurrentDictionary<int, List<MensajeMediciones>>();

        [HttpPost("{nodeId:int}/alive")]
        public IActionResult Alive(int nodeId)
        {
            return Ok();
        }

        [HttpPost("{nodeId:int}/measurements")]
        public IActionResult PostMeasurement(int nodeId, [FromBody] MensajeMediciones medicion)
        {
            if (medicion == null)
                return BadRequest("Medicion is null");

            if(mensajesPorNodo.ContainsKey(nodeId) == false) {
                mensajesPorNodo.TryAdd(nodeId, new List<MensajeMediciones>());
            }

            mensajesPorNodo[nodeId].Add(medicion);

            Console.WriteLine(JsonSerializer.Serialize(medicion));

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
