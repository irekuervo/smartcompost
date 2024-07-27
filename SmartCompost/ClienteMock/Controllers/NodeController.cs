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
        private static ConcurrentDictionary<string, List<MedicionesNodoDto>> mensajesPorNodo = new ConcurrentDictionary<string, List<MedicionesNodoDto>>();

        [HttpPost("{serialNumber:string}/alive")]
        public IActionResult Alive(string serialNumber)
        {
            return Ok();
        }

        [HttpPost("{serialNumber:string}/measurements")]
        public IActionResult PostMeasurement(string serialNumber, [FromBody] MedicionesNodoDto medicion)
        {
            if (medicion == null)
                return BadRequest("Medicion is null");

            if(mensajesPorNodo.ContainsKey(serialNumber) == false) {
                mensajesPorNodo.TryAdd(serialNumber, new List<MedicionesNodoDto>());
            }

            mensajesPorNodo[serialNumber].Add(medicion);

            Console.WriteLine(JsonSerializer.Serialize(medicion));

            ultimoMensajeRecibido = DateTime.Now;

            return Ok();
        }

        [HttpPost]
        public IActionResult Create([FromBody] NodoDto nodo)
        {
            //TERMINAR
            return Ok(new NodoDto());
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
