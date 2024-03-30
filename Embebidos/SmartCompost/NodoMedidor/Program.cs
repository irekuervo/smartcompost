using NanoKernel;
using System.Reflection;

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
