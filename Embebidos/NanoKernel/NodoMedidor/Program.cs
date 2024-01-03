using NanoKernel;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace NodoMedidor
{
    public class Program
    {
        public static void Main()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            App.Start(assembly);
        }
    }
}
