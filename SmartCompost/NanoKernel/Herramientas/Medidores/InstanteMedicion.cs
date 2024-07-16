using System.Collections;

namespace NanoKernel.Herramientas.Medidores
{
    public class InstanteMedicion
    {
        public Medicion MedicionTiempoMilis { get; set; }
        public Hashtable Contadores = new Hashtable();
        public Hashtable Mediciones = new Hashtable();

        public ulong ContadoEnPeriodo(string nombreContador)
        {
            if (Contadores.Contains(nombreContador))
                return ((Contador)Contadores[nombreContador]).ContadorEnPeriodo;

            return 0;
        }

        public ulong ContadoTotal(string nombreContador)
        {
            if (Contadores.Contains(nombreContador))
                return ((Contador)Contadores[nombreContador]).ContadorTotal;

            return 0;
        }

    }
}
