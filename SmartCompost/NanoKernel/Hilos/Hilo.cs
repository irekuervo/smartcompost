using NanoKernel.Logging;
using System;
using System.Threading;

namespace NanoKernel.Hilos
{
    public delegate void HiloDelegate(ref bool hiloActivo);

    public class HiloInfo
    {
        public string Id;
        public string Nombre;
        public string Descripcion;
        public DateTime FechaCreacion;
        public DateTime FechaUltimaEjecucion;
    }

    public class Hilo : IDisposable
    {
        public ThreadState Estado => tarea.ThreadState;
        public Guid Id { get; private set; }
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }
        public string MetodoCreador { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public DateTime FechaUltimaEjecucion { get; private set; }
        public DateTime FechaUltimoDetenimiento { get; private set; }
        public int MilisTimeoutDetenimiento { get; private set; }
        public int MaxCantidadReintentos { get; private set; }
        public int MilisEsperaReintentos { get; private set; }
        public int CantidadReintentos { get; private set; }


        internal Thread tarea;

        private HiloDelegate delegado;

        private bool activo;

        internal Hilo(Guid id,
            string nombre,
            string descripcion,
            HiloDelegate delegado,
            string metodoCreador,
            int milisTimeoutDetenimiento,
            int cantidadReintentos,
            int milisEsperaReintentos
            )
        {
            this.Id = id;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.FechaCreacion = DateTime.UtcNow;
            this.MetodoCreador = metodoCreador;
            this.MilisTimeoutDetenimiento = milisTimeoutDetenimiento;
            this.CantidadReintentos = cantidadReintentos;
            this.MilisEsperaReintentos = milisEsperaReintentos;

            this.delegado = delegado;
        }

        public void Iniciar()
        {
            Detener();

            activo = true;
            tarea = new Thread(() =>
            {
                bool terminoOk = false;
                try
                {
                    FechaUltimaEjecucion = DateTime.UtcNow;
                    Logger.Log($"{this} ejecutado");
                    delegado(ref activo);
                    FechaUltimoDetenimiento = DateTime.UtcNow;
                    terminoOk = true;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                }
                finally
                {
                    activo = false;
                    if (terminoOk == false && MaxCantidadReintentos > 0)
                    {
                        Logger.Log($"{this} detenido Error");
                        new Thread(Reintentar).Start();
                    }
                    else
                    {
                        Logger.Log($"{this} detenido OK");
                        reintentando = false; CantidadReintentos = 0;
                    }
                }
            });
            tarea.Start();
        }

        bool reintentando = false;
        private void Reintentar()
        {
            if (reintentando)
                return;

            CantidadReintentos++;

            if (CantidadReintentos == MaxCantidadReintentos)
            {
                Logger.Log($"{this} no pudo ejecutarse correctamente luego de {CantidadReintentos} reintentos");
                reintentando = false;
                CantidadReintentos = 0;
                return;
            }

            Thread.Sleep(MilisEsperaReintentos);
            Logger.Log($"{this} reintento n°{CantidadReintentos}");
            Iniciar();
        }

        public void Detener()
        {
            activo = false;

            if (tarea == null)
                return;

            if (tarea.Join(MilisTimeoutDetenimiento) == false)
                throw new Exception($"No se pudo detener {this}");
        }

        public static void Intentar(Action accion, string nombreIntento = "", int milisIntento = 1000, uint intentos = uint.MaxValue)
        {
            Logger.Debug("Intentando " + nombreIntento);
            var ok = false;
            while (!ok && intentos > 0)
            {
                try
                {
                    accion.Invoke();
                    Logger.Debug($"{nombreIntento} OK");
                    ok = true;
                }
                catch (Exception)
                {
                    Logger.Debug("Reintentando");
                    Thread.Sleep(milisIntento);
                    ok = false;
                }
                finally
                {
                    intentos--;
                }
            }
        }

        public override string ToString()
        {
            return $"Hilo [{Id}]->{Nombre}";
        }

        public void Dispose()
        {
            Detener();
        }
    }
}