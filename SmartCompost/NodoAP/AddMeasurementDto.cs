using System;

namespace NodoAP
{
    public class AddMeasurementDto
    {
        public string node_type { get; set; }
        public DateTime timestamp { get; set; } // Now del nodo, DATO DEBUG TODO, es para saber el tiempo de transporte
        public Array node_measurements { get; set; }
    }

    public class MeasurementDto
    {
        public DateTime timestamp { get; set; } // Now de la medicion
        public string value { get; set; }
        public string type { get; set; }

    }
}
