using System;

namespace PruebaAP
{
    public static class Logger
    {
        public static void Debug(string mensaje)
        {
#if DEBUG
            LogInternal(mensaje);
#endif
        }

        public static void Log(string mensaje)
        {
            LogInternal(mensaje);
        }

        public static void Error(string mensaje)
        {
            LogInternal($"ERROR: {mensaje}");
        }

        public static void Log(Exception ex)
        {
            LogInternal($"{ex.Message} \r\n {ex.StackTrace}");
        }

        public static void LogInternal(string mensaje)
        {
            Console.WriteLine($"{DateTime.UtcNow} | {mensaje}");
        }
    }
}
