using System.Collections;

namespace MockSmartcompost.Dto
{
    public class MensajeMediciones
    {
        public DateTime last_updated { get; set; }
        public List<Medicion> node_measurements { get; set; } = new List<Medicion>();
    }

    public class Medicion
    {
        public float value { get; set; }
        public DateTime timestamp { get; set; }
        public string type { get; set; }
    }
}
