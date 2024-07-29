using System;
using System.Collections;

namespace NanoKernel.DTOs
{
    public class MedicionesNodoDto
    {
        public string serial_number { get; set; }
        public DateTime last_updated { get; set; }
        public ArrayList measurements { get; set; } = new ArrayList();
    }

    public class Medicion
    {
        public float value { get; set; }
        public DateTime timestamp { get; set; }
        public string type { get; set; }
    }
}
