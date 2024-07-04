using System;
using System.Collections;

namespace NanoKernel.Comunicacion
{
    public class AddMeasurementDto
    {
        // iso8601
        public DateTime last_updated { get; set; } // Now del nodo, DATO DEBUG TODO, es para saber el tiempo de transporte
        public ArrayList node_measurements { get; set; }
    }

    public class MeasurementDto
    {
        public DateTime timestamp { get; set; } // Now de la medicion
        public float value { get; set; }
        public string type { get; set; }

    }
}
