﻿using System;
using System.Diagnostics;

namespace NanoKernel.Loggin
{
    public static class Logger
    {
        public const bool LogDebug = true;
        public static void Log(string mensaje)
        {
            if (LogDebug)
            {
                Debug.WriteLine($"{DateTime.UtcNow} | {mensaje}");
            }
        }
    }
}
