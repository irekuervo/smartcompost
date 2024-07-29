using System.Collections;

namespace NanoKernel.Herramientas.Repositorios
{
    public class CacheClaveValor : IRepositorioClaveValor
    {
        public CacheClaveValor()
        {

        }

        public CacheClaveValor(Hashtable valoresIniciales)
        {
            Tabla = valoresIniciales;
        }

        public readonly Hashtable Tabla = new Hashtable();

        private readonly object cacheLock = new object();

        public void Delete(string id)
        {
            lock (cacheLock)
            {
                if (Tabla.Contains(id))
                {
                    Tabla.Remove(id);
                }
            }
        }

        public string Get(string id)
        {
            lock (cacheLock)
            {
                if (!Tabla.Contains(id))
                    return null;

                return Tabla[id] as string;
            }
        }

        public string[] GetAll()
        {
            lock (cacheLock)
            {
                string[] values = new string[Tabla.Count];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = Tabla[i] as string;
                }
                return values;
            }
        }

        public void Update(string id, string value)
        {
            lock (cacheLock)
            {
                if (Tabla.Contains(id) == false)
                    Tabla.Add(id, value);
                else
                    Tabla[id] = value;
            }
        }

        public void Dispose()
        {
            Tabla.Clear();
        }
    }
}
