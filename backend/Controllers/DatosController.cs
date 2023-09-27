using Microsoft.AspNetCore.Mvc;

namespace CreativeTim.Argon.DotNetCore.Free.Controllers
{
    [Route("api/datos")]
    [ApiController]
    public class DatosController : Controller
    {

        static List<Dato> datosNodo = new List<Dato>();

        [HttpGet]
        public IActionResult Get()
        {
            var random = new Random();

            datosNodo.Add(new Dato { Id = datosNodo.Count, Valor = random.Next(1, 100) });

            return Ok(datosNodo);
        }
    }



    public class Dato
    {
        public int Id { get; set; }
        public int Valor { get; set; }
    }
}
