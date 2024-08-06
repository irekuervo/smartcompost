using nanoFramework.Hardware.Esp32;

namespace NanoKernel.Ayudantes
{
    public static class AyMemoria
    {
        public static uint GC_Run(bool compactHeap)
        {
            uint freeBytes = nanoFramework.Runtime.Native.GC.Run(compactHeap);
            return freeBytes;
        }

        public static void GetMemory(out uint totalsize, out uint totalFree, out uint largestFreeBlock)
        {
            NativeMemory.GetMemoryInfo(
                NativeMemory.MemoryType.Internal,
                out totalsize,
                out totalFree,
                out largestFreeBlock);
        }
    }
}
