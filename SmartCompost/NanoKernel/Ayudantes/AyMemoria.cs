using NanoKernel.Logging;

namespace NanoKernel.Ayudantes
{
    public static class AyMemoria
    {
        public static uint GC_Run(bool compactHeap)
        {
            uint freeBytes = nanoFramework.Runtime.Native.GC.Run(compactHeap);
            return freeBytes;
        }
    }
}
