using NanoKernel.Logging;
using System.Threading;

namespace NodoAP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new NodoAP().Iniciar();
            Logger.Log("FIN");
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
