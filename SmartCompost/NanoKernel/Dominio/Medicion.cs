using System;
using System.Collections;

namespace NanoKernel.Dominio
{
    public class MensajeMediciones
    {
        public DateTime last_updated;
        public ArrayList node_measurments = new ArrayList();
    }

    public class Medicion
    {
        public float value;
        public DateTime timestamp;
        public string type;
    }
}
