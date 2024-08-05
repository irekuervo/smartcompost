using nanoFramework.Hardware.Esp32;
using NanoKernel.Ayudantes;
using NanoKernel.Dominio;
using NanoKernel.Logging;
using System;

namespace NanoKernel.Nodos
{
    public abstract class NodoBase : IDisposable
    {
        public MacAddress MacAddress { get; private set; }
        public ConfigNodo Config { get; private set; }

        //public readonly EstadisticaEscalar EstadisticaRAM;

        public abstract TiposNodo tipoNodo { get; }

        private const string LOGO = "\r\n\r\n  ______  _____  _                        _ \r\n |  ____|/ ____|| |                      | |\r\n | |__  | (___  | | _____ _ __ _ __   ___| |\r\n |  __|  \\___ \\ | |/ / _ \\ '__| '_ \\ / _ \\ |\r\n | |____ ____) ||   <  __/ |  | | | |  __/ |\r\n |______|_____(_)_|\\_\\___|_|  |_| |_|\\___|_|\r\n                                            \r\n                                            \r\n\r\n";
        private const string dirConfigNodo = ayArchivos.DIR_INTERNO + ConfigNodo.NombreArchivo;

        private bool activo = false;

        public abstract void Loop(ref bool activo);
        public abstract void Setup();

        public void Iniciar()
        {
            activo = true;

            Logger.Log(LOGO);

            Sleep.WakeupCause cause = Sleep.GetWakeupCause();
            Logger.Log("Wakeup cause: " + cause.ToString());

            Logger.Log("------------Setup------------");

            MacAddress = ayInternet.GetMacAddress();
            Logger.Log($"MAC: {MacAddress}");

            //EstadisticaRAM = new EstadisticaEscalar();

            try
            {
                Config = (ConfigNodo)ayArchivos.AbrirJson(dirConfigNodo, typeof(ConfigNodo));
            }
            catch (Exception ex)
            {
                Config = ConfigNodo.Default();
                Logger.Error("No se pudo cargar la conf: " + ex.Message);
            }

            Logger.Log($"{Config.ToString()}");
           
            Setup();

            Logger.Log("------------Loop------------");

            while (activo)
            {
                Loop(ref activo);
            }
        }

        public void Detener()
        {
            activo = false;
        }

        public virtual void Dispose()
        {
            Detener();
        }

        protected void LimpiarMemoria()
        {
            uint freeMemory = AyMemoria.GC_Run(true);
            //EstadisticaRAM.AgregarMuestra(freeMemory);
            //Logger.Debug($"Free RAM: {EstadisticaRAM.UltimaMuestra} | Max: {EstadisticaRAM.Maximo} | Min: {EstadisticaRAM.Minimo} | mu: {EstadisticaRAM.Promedio()} sigma: {EstadisticaRAM.Desvio()} ");
        }
    }
}
