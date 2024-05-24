using nanoFramework.Hardware.Esp32;
using nanoFramework.Runtime.Native;
using NanoKernel.Loggin;
using System;

namespace NanoKernel.Ayudantes
{
    public class aySleep

    {
        /// <summary>
        /// Mantiene el equipo en deep sleep, y rebootea
        /// </summary>
        /// <param name="segundos"></param>
        /// <param name="razon"></param>
        public static void DeepSleepSegundos(int segundos, string razon = "")
        {
            razon = razon != "" ? "\r\n" + razon : razon;
            Logger.Log("Deep sleep por " + segundos + "segs. " + razon);
            Sleep.EnableWakeupByTimer(new TimeSpan(0, 0, 0, segundos));
            Sleep.StartDeepSleep();
        }

    }
}
