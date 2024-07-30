using nanoFramework.Hardware.Esp32;
using NanoKernel.Ayudantes;
using NanoKernel.Dominio;
using NanoKernel.Herramientas.Estadisticas;
using NanoKernel.Hilos;
using NanoKernel.Logging;
using System;
using System.Threading;

namespace NanoKernel.Nodos
{
    public abstract class NodoBase : IDisposable
    {
        public readonly MacAddress MacAddress;
        public readonly ConfigNodo Config;
        public readonly EstadisticaEscalar EstadisticaRAM;

        public abstract TiposNodo tipoNodo { get; }

        private readonly Hilo loopThread;
        
        private const string LOGO = "\r\n\r\n  ______  _____  _                        _ \r\n |  ____|/ ____|| |                      | |\r\n | |__  | (___  | | _____ _ __ _ __   ___| |\r\n |  __|  \\___ \\ | |/ / _ \\ '__| '_ \\ / _ \\ |\r\n | |____ ____) ||   <  __/ |  | | | |  __/ |\r\n |______|_____(_)_|\\_\\___|_|  |_| |_|\\___|_|\r\n                                            \r\n                                            \r\n\r\n";
        private const string dirConfigNodo = ayArchivos.DIR_INTERNO + ConfigNodo.NombreArchivo;
        public NodoBase()
        {
            loopThread = MotorDeHilos.CrearHiloLoop($"MAIN LOOP", Loop);
            MacAddress = ayInternet.GetMacAddress();
            EstadisticaRAM = new EstadisticaEscalar();

            Config = ConfigNodo.Default(); // Iniciamos un config por default
            if (ayArchivos.ArchivoExiste(dirConfigNodo) == false)
            {
                Logger.Error("No se pudo cargar la conf " + dirConfigNodo);
                return;
            }

            try
            {
                Config = (ConfigNodo)ayArchivos.AbrirJson(dirConfigNodo, typeof(ConfigNodo));
            }
            catch (Exception ex)
            {
                Logger.Error("No se pudo cargar la conf: " + ex.Message);
            }
        }

        public abstract void Loop(ref bool activo);
        public abstract void Setup();

        public void Iniciar()
        {
            Logger.Log(LOGO);
            Logger.Log($"{Config.NumeroSerie}");
            Logger.Log($"MAC: {MacAddress}");

            Sleep.WakeupCause cause = Sleep.GetWakeupCause();
            Logger.Log("Wakeup cause: " + cause.ToString());

            LimpiarMemoria();

            Logger.Log("Setup...");

            Setup();

            Logger.Log("Starting loop...");
            loopThread.Iniciar();

            Logger.Log("------------Nodo iniciado------------");
            Thread.Sleep(Timeout.Infinite);
        }

        public void Detener()
        {
            loopThread.Detener();
        }

        public virtual void Dispose()
        {
            Detener();
            loopThread.Dispose();
        }

        protected void LimpiarMemoria()
        {
            EstadisticaRAM.AgregarMuestra(AyMemoria.GC_Run(true));
            Logger.Debug($"Free RAM: {EstadisticaRAM.UltimaMuestra} | Max: {EstadisticaRAM.Maximo} | Min: {EstadisticaRAM.Minimo} | mu: {EstadisticaRAM.Promedio()} sigma: {EstadisticaRAM.Desvio()} ");
        }
    }
}
