using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace NanoKernel.Herramientas.Medidores
{
    public delegate void MedicionEnPeriodoCallback(InstanteMedicion resultado);

    public class Medidor : IDisposable
    {
        public event MedicionEnPeriodoCallback OnMedicionesEnPeriodoCallback;

        public bool Activo { get; private set; }
        public Medicion MedicionTiempoMilis { get; private set; }
        public Hashtable Contadores { get; } = new Hashtable();
        public Hashtable Mediciones { get; } = new Hashtable();

        private Timer timer;
        private Stopwatch stopwatch;
        private int milisPeriodoTimer;

        private InstanteMedicion bufferPendiente = null;
        private InstanteMedicion bufferProcesado = null;
        private Thread workerThread;
        private AutoResetEvent procesamientoDisponible = new AutoResetEvent(false); // Notifica cuando hay trabajo pendiente

        private object lockGlobal = new object();

        public Medidor(int milisPeriodoTimer = 1000)
        {
            if (milisPeriodoTimer < 1)
                milisPeriodoTimer = 1;

            this.milisPeriodoTimer = milisPeriodoTimer;
            MedicionTiempoMilis = new Medicion();
            stopwatch = new Stopwatch();
        }

        public void Iniciar()
        {
            if (Activo)
                return;

            Activo = true;
            stopwatch.Start();
            timer = new Timer(OnTimerPeriodo, null, milisPeriodoTimer, milisPeriodoTimer);

            // Iniciar hilo de procesamiento con AutoResetEvent
            workerThread = new Thread(ProcesarMediciones);
            workerThread.Start();
        }

        public void Detener()
        {
            if (!Activo)
                return;

            Activo = false;
            timer?.Dispose();
            stopwatch.Stop();

            OnTimerPeriodo(null); // Captura la última medición antes de detener
            procesamientoDisponible.Set(); // Despierta el workerThread para que termine
            workerThread.Join();
        }

        public void Contar(string clave, int cantidad = 1)
        {
            if(cantidad <= 0) return;

            // BORRAR
            if (cantidad > 1000) 
                return;

            lock (lockGlobal)
            {
                if (!Contadores.Contains(clave))
                    Contadores.Add(clave, new Contador());

                ((Contador)Contadores[clave]).Contar(cantidad);
            }
        }

        public void Medir(string clave, float muestra)
        {
            lock (lockGlobal)
            {
                if (!Mediciones.Contains(clave))
                    Mediciones.Add(clave, new Medicion());

                ((Medicion)Mediciones[clave]).AgregarMuestra(muestra);
            }
        }

        public Medicion ObtenerMedicion(string nombreMedicion)
        {
            lock (lockGlobal)
            {
                if (Mediciones.Contains(nombreMedicion))
                    return (Medicion)Mediciones[nombreMedicion];

                return null;
            }
        }

        public int ContadoEnPeriodo(string nombreContador)
        {
            lock (lockGlobal)
            {
                if (Contadores.Contains(nombreContador))
                    return ((Contador)Contadores[nombreContador]).ContadorEnPeriodo;

                return 0;
            }
        }

        public int ContadoTotal(string nombreContador)
        {
            lock (lockGlobal)
            {
                if (Contadores.Contains(nombreContador))
                    return ((Contador)Contadores[nombreContador]).ContadorTotal;

                return 0;
            }
        }

        private void OnTimerPeriodo(object state)
        {
            lock (lockGlobal)
            {
                stopwatch.Stop();
                float tiempoMedido = (float)stopwatch.Elapsed.TotalMilliseconds;
                MedicionTiempoMilis.AgregarMuestra(tiempoMedido);
                stopwatch.Reset();

                InstanteMedicion nuevaMedicion = new InstanteMedicion
                {
                    MedicionTiempoMilis = MedicionTiempoMilis.Clonar(),
                    Contadores = ClonarContadores(),
                    Mediciones = ClonarMediciones()
                };

                bufferPendiente = nuevaMedicion;

                procesamientoDisponible.Set(); // Notifica al workerThread que hay datos listos

                // Limpiar datos del periodo anterior
                MedicionTiempoMilis.MedicionEnPeriodo.Limpiar();

                foreach (var item in Contadores.Values)
                    ((Contador)item).LimpiarEnPeriodo();

                foreach (var item in Mediciones.Values)
                    ((Medicion)item).MedicionEnPeriodo.Limpiar();

                stopwatch.Start(); // Se inicia después de limpiar, sin perder eventos
            }
        }

        private void ProcesarMediciones()
        {
            while (Activo)
            {
                procesamientoDisponible.WaitOne(); // Espera hasta que haya datos listos

                lock (lockGlobal)
                {
                    if (bufferPendiente != null)
                    {
                        bufferProcesado = bufferPendiente;
                        bufferPendiente = null;
                    }
                }

                if (bufferProcesado != null && OnMedicionesEnPeriodoCallback != null)
                {
                    OnMedicionesEnPeriodoCallback(bufferProcesado);
                    bufferProcesado = null;
                }
            }
        }

        private Hashtable ClonarMediciones()
        {
            Hashtable res = new Hashtable();
            foreach (var item in Mediciones.Keys)
            {
                res.Add(item, ((Medicion)Mediciones[item]).Clonar());
            }
            return res;
        }

        private Hashtable ClonarContadores()
        {
            Hashtable res = new Hashtable();
            foreach (var item in Contadores.Keys)
            {
                res.Add(item, ((Contador)Contadores[item]).Clonar());
            }
            return res;
        }

        public void Dispose()
        {
            Detener();
        }
    }
}