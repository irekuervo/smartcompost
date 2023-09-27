using CreativeTim.Argon.DotNetCore.Free.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace smartcompost.Controllers
{
    [Route("api/datos2")]
    [ApiController]
    public class Datos2Controller : Controller
    {
        static GraficoLinea grafico = new GraficoLinea();

        [HttpGet]
        public IActionResult Get()
        {
            var random = new Random();

            grafico.datasets.Add(random.Next(1, 100));
            grafico.labels.Add(DateTime.Now.ToString("g"));

            return Ok(grafico);
        }
    }

    public class GraficoLinea
    {
        public List<int> datasets { get; set; } = new List<int>();
        public List<string> labels { get; set; } = new List<string>();
    }
}
