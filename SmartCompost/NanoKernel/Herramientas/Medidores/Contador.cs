using System.Threading;

namespace NanoKernel.Herramientas.Medidores
{
    /// <summary>
    /// Usamos interlocked para evitar salvar concurrencia con locking, ya que son operaciones
    /// atomicas muy recurrentes y sencillas
    /// </summary>
    public class Contador
    {
        public int ContadorEnPeriodo => Interlocked.Exchange(ref contadorEnPeriodo, contadorEnPeriodo);
        public int ContadorTotal => Interlocked.Exchange(ref contadorTotal, contadorTotal);

        private int contadorEnPeriodo = 0;
        private int contadorTotal = 0;

        public void Contar(int cantidad = 1)
        {
            Incrementar(ref contadorEnPeriodo, cantidad);
            Incrementar(ref contadorTotal, cantidad);
        }

        private void Incrementar(ref int variable, int cantidad)
        {
            int original, nuevoValor;
            do
            {
                original = Interlocked.Exchange(ref variable, variable);
                nuevoValor = original + cantidad;
                if (nuevoValor < original) nuevoValor = int.MaxValue; // Evita overflow
            } while (Interlocked.CompareExchange(ref variable, nuevoValor, original) != original);
        }

        public void LimpiarEnPeriodo()
        {
            Interlocked.Exchange(ref contadorEnPeriodo, 0);
        }

        public Contador Clonar()
        {
            return new Contador
            {
                contadorEnPeriodo = Interlocked.Exchange(ref contadorEnPeriodo, contadorEnPeriodo),
                contadorTotal = Interlocked.Exchange(ref contadorTotal, contadorTotal)
            };
        }
    }
}
