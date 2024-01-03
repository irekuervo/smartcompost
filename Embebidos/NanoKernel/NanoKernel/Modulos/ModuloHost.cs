using nanoFramework.Runtime.Native;
using NanoKernel.Loggin;

namespace NanoKernel.Modulos
{
    [Modulo("Host")]
    public class ModuloHost
    {
        public ModuloHost()
        {
            Power.OnRebootEvent += Power_OnRebootEvent;
        }

        private void Power_OnRebootEvent()
        {
            Logger.Log("REBOOT");
        }

        [Servicio("Memoria")]
        public uint Memoria(bool compactHeap)
        {
            return GC.Run(compactHeap);
        }

        [Servicio("Reboot")]
        public void Reboot(int timeout)
        {
            Logger.Log($"Restart manual, {timeout}ms timeout");
            Power.RebootDevice(timeout);
        }
    }
}
