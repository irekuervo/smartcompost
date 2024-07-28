using NanoKernel.Nodos;

namespace NodoTest
{
    public class Program
    {
        public static void Main()
        {
            new Nodo().Iniciar();
        }

        public class Nodo : NodoBase
        {
            public override TiposNodo tipoNodo => TiposNodo.Generico;

            public override void Setup()
            {
            }

            public override void Loop(ref bool activo)
            {
            }
        }
    }
}
