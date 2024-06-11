using NanoKernel;
using NanoKernel.Modulos;
using System.Threading;

namespace NodoTest
{
    public class Program
    {
        static ModuloBlinkLed blinker;
        public static void Main()
        {
            blinker = new ModuloBlinkLed();
            blinker.Iniciar();
        }

        static void setup()
        {
            blinker = new ModuloBlinkLed();
            blinker.Iniciar();
        }

        static void loop(ref bool activo)
        {
            Thread.Sleep(1000);
        }
    }
}
