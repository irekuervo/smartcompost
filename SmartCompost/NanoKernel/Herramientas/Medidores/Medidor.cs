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
        public Medicion MedicionTiempoMilis { get; set; }
        public Hashtable Contadores = new Hashtable();
        public Hashtable Mediciones = new Hashtable();

        private Timer timer;
        private Stopwatch stopwatch;
        private int milisPeriodoTimer;
        private object lockObj = new object();

        /// <summary>
        /// La idea de esta clase es tener intervalos iguales de tiempo para el cual se hacen mediciones
        /// y se registran resultados. Actualmente se miden tiempo de ejecucion y cantidad de un contador,
        /// </summary>
        /// <param name="milisPeriodoTimer"></param>
        public Medidor(int milisPeriodoTimer = 1000)
        {
            if (milisPeriodoTimer < 0)
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

            timer = new Timer(TimerCallback, null, milisPeriodoTimer, milisPeriodoTimer);
        }

        public void Detener()
        {
            if (!Activo)
                return;

            if (timer != null)
                timer.Dispose();

            stopwatch.Stop();

            TimerCallback(null);

            Activo = false;
        }

        public void IniciarMedicionDeTiempo()
        {
            lock (lockObj)
            {
                if (timer == null)
                    Iniciar();

                stopwatch.Start();
            }
        }

        float medicionDeTiempoMilis = 0;
        public double FinalizarMedicionDeTiempo()
        {
            lock (lockObj)
            {
                stopwatch.Stop();
                medicionDeTiempoMilis = (float)stopwatch.Elapsed.TotalMilliseconds;
                MedicionTiempoMilis.AgregarMuestra(medicionDeTiempoMilis);
                stopwatch.Reset();

                return medicionDeTiempoMilis;
            }
        }

        public void Contar(string clave, int cantidad = 1) => Contar(clave, (ulong)cantidad);

        public void Contar(string clave, ulong cantidad)
        {
            lock (lockObj)
            {
                if (Contadores.Contains(clave) == false)
                    Contadores.Add(clave, new Contador());

                ((Contador)Contadores[clave]).Contar(cantidad);
            }
        }

        public void Medir(string clave, float muestra)
        {
            lock (lockObj)
            {
                if (Mediciones.Contains(clave) == false)
                    Mediciones.Add(clave, new Medicion());

                ((Medicion)Mediciones[clave]).AgregarMuestra(muestra);
            }
        }

        public Medicion Medicion(string nombreMedicion)
        {
            if (Mediciones.Contains(nombreMedicion))
                return (Medicion)Mediciones[nombreMedicion];

            return null;
        }

        public ulong ContadoEnPeriodo(string nombreContador)
        {
            if (Contadores.Contains(nombreContador))
                return ((Contador)Contadores[nombreContador]).ContadorEnPeriodo;

            return 0;
        }

        public ulong ContadoTotal(string nombreContador)
        {
            if (Contadores.Contains(nombreContador))
                return ((Contador)Contadores[nombreContador]).ContadorTotal;

            return 0;
        }

        public void Limpiar()
        {
            lock (lockObj)
            {
                foreach (var item in Mediciones.Values)
                {
                    ((Medicion)item).Limpiar();
                }
            }
        }

        // Clock del timer en un periodo
        private void TimerCallback(object state)
        {
            lock (lockObj)
            {
                if (stopwatch.IsRunning)
                {
                    FinalizarMedicionDeTiempo();
                }

                if (OnMedicionesEnPeriodoCallback != null)
                {
                    InstanteMedicion res = new InstanteMedicion();
                    res.MedicionTiempoMilis = MedicionTiempoMilis.Clonar();
                    res.Contadores = ClonarContadores();
                    res.Mediciones = ClonarMediciones();
                    OnMedicionesEnPeriodoCallback(res);
                }

                // limapiamos todo lo repectivo al periodo
                // Medicion de tiempos
                MedicionTiempoMilis.MedicionEnPeriodo.Limpiar();

                // Contadores
                foreach (var item in Contadores.Values)
                {
                    ((Contador)item).ContadorEnPeriodo = 0;
                }

                // Mediciones
                foreach (var item in Mediciones.Values)
                {
                    ((Medicion)item).MedicionEnPeriodo.Limpiar();
                }

                IniciarMedicionDeTiempo();
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
