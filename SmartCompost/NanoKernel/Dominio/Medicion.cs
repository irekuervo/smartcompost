using NanoKernel.Ayudantes;
using NanoKernel.Comunicacion;
using System;
using System.Collections;

namespace NanoKernel.Dominio
{
    public class MensajeMediciones
    {
        public DateTime last_updated { get; set; }
        public ArrayList node_measurements { get; set; } = new ArrayList();
    }

    public class Medicion
    {
        public float value { get; set; }
        public DateTime timestamp { get; set; }
        public string type { get; set; }
    }
}
