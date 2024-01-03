using NanoKernel.Modulos;
using System;

namespace NodoMedidor
{
    [Modulo("Sensores")]
    public class ModuloSensor
    {
        private static Random random = new Random();

        [Servicio("Temperatura")]
        public double Temperatura()
        {
            return random.NextDouble() * 100;
        }
    }
}
