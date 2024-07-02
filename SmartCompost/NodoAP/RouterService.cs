using NanoKernel.Hilos;
using System;

namespace NodoAP
{
    public class RouterService : IDisposable
    {
        private Hilo hiloColaSalida;
        public RouterService()
        {

        }

        public void Iniciar()
        {
            hiloColaSalida = MotorDeHilos.CrearHiloLoop("RouterService", loopColaSalida);
        }

        void loopColaSalida(ref bool activo)
        {

        }

        public void Detener()
        {
            hiloColaSalida.Detener();
        }

        public void Dispose()
        {
            Detener();
        }
    }
}
