using nanoFramework.Hardware.Esp32;
using NanoKernel.Ayudantes;
using NanoKernel.Dominio;
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

        public abstract TiposNodo tipoNodo { get; }

        private readonly Hilo loopThread;

        private const string LOGO = "\r\n\r\n  ______  _____  _                        _ \r\n |  ____|/ ____|| |                      | |\r\n | |__  | (___  | | _____ _ __ _ __   ___| |\r\n |  __|  \\___ \\ | |/ / _ \\ '__| '_ \\ / _ \\ |\r\n | |____ ____) ||   <  __/ |  | | | |  __/ |\r\n |______|_____(_)_|\\_\\___|_|  |_| |_|\\___|_|\r\n                                            \r\n                                            \r\n\r\n";
        private const string dirInfoNodo = ayArchivos.DIR_INTERNO + "infoNodo.json";
        public NodoBase()
        {
            loopThread = MotorDeHilos.CrearHiloLoop($"MAIN LOOP", Loop);
            MacAddress = ayInternet.GetMacAddress();

            Config = ConfigNodo.Default(); // Iniciamos un config por default
            if (ayArchivos.ArchivoExiste(dirInfoNodo) == false)
            {
                Logger.Error("No se pudo cargar la conf " + dirInfoNodo);
                return;
            }

            try
            {
                Config = (ConfigNodo)ayArchivos.AbrirJson(dirInfoNodo, typeof(ConfigNodo));
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

    }
}
