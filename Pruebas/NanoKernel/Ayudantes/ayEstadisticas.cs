using System;

namespace NanoKernel.Ayudantes
{
    public class ayEstadisticas
    {
        public static double ObtenerDesvioEstandar(float cantidadMuestras, float sumaCuadratica, float suma, float promedio)
        {
            if (cantidadMuestras == 0)
                return 0;

            return Math.Sqrt((1.0 / cantidadMuestras) * (sumaCuadratica - (2 * promedio * suma) + (promedio * promedio) * cantidadMuestras));
        }
    }
}
