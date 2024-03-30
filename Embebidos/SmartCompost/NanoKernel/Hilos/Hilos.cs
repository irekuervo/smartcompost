using NanoKernel.Loggin;
using System;
using System.Collections;

namespace NanoKernel.Hilos
{
    // ES ESTATICO PORQUE ES DEMASIADO CRITICO DEJAR HILOS ABIERTOS, CENTRALIZO TODO
    public static class Hilos
    {
        public static ICollection GetHilos => hilos.Values;

        private static Hashtable hilos = new Hashtable();

        public static Hilo CrearHilo(string nombreHilo,
            HiloDelegate delegado,
            string descripcionHilo = "",
            int milisTimeoutDetenimiento = 10_000,
            int cantidadReintentos = 0,
            int milisEsperaReintentos = 0,
            string metodoCreador = "")
        {
            Hilo hilo = new Hilo(
                Guid.NewGuid(),
                nombreHilo,
                descripcionHilo,
                delegado,
                metodoCreador,
                milisTimeoutDetenimiento,
                cantidadReintentos,
                milisEsperaReintentos);

            RegistrarHilo(hilo);

            return hilo;
        }

        public static Hilo CrearHiloLoop(
            string nombreHilo,
            HiloDelegate tareaLoop,
            Action tareaInicializacion = null,
            Action tareaFinalizacion = null,
            string descripcionHilo = "",
            int milisTimeoutDetenimiento = 10000,
            int cantidadReintentos = 5,
            int milisEsperaReintentos = 1000
            )
        {
            HiloDelegate delegadoTarea = (ref bool activo) =>
            {
                tareaInicializacion?.Invoke();

                while (activo)
                {
                    tareaLoop(ref activo);
                }

                tareaFinalizacion?.Invoke();
            };

            return CrearHilo(nombreHilo, delegadoTarea, descripcionHilo, milisTimeoutDetenimiento, cantidadReintentos, milisEsperaReintentos);
        }

        public static void EliminarHilo(Hilo hilo)
        {
            if (hilo == null)
                return;

            hilo.Dispose();

            if (hilos.Contains(hilo.Id) == false)
                return;

            hilos.Remove(hilo);
        }

        private static void RegistrarHilo(Hilo hilo)
        {
            hilos.Add(hilo.Id, hilo);
            Logger.Log("Hilo creado: " + hilo.ToString());
        }

        public static void Dispose()
        {
            try
            {
                foreach (var hilo in hilos)
                {
                    Logger.Log("Deteniendo " + hilo.ToString());
                    (hilo as Hilo).Dispose();
                }

                foreach (var item in hilos)
                {
                    var hilo = item as Hilo;
                    if (hilo.Estado == System.Threading.ThreadState.Stopped)
                        hilos.Remove(item);
                    else
                        Logger.Log("Hilo no se pudo cerrar: " + item.ToString());
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
