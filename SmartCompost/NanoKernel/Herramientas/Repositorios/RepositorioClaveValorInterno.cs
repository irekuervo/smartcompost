using NanoKernel.Ayudantes;
using System.Collections;
using System.IO;

namespace NanoKernel.Herramientas.Repositorios
{
    /// <summary>
    /// Herramienta para manejar un repo en la unidad I: propia del micro
    /// Esto usa la memoria EEPROM
    /// </summary>
    public class RepositorioClaveValorInterno : IRepositorioClaveValor
    {
        public const string DireccionDbClaveValor = ayArchivos.DIR_INTERNO + @"db_clave_valor\";

        private string pathDb;

        private CacheClaveValor cache = new CacheClaveValor();

        public RepositorioClaveValorInterno(string nombre)
        {
            pathDb = DireccionDbClaveValor + nombre;

            InicializarDb();
        }

        public void Update(string id, string value)
        {
            cache.Update(id, value);
            ActualizarRepositorio();
        }

        public void Delete(string id)
        {
            cache.Delete(id);
            ActualizarRepositorio();
        }

        public string Get(string id)
        {
            return cache.Get(id);
        }

        public string[] GetAll()
        {
            return cache.GetAll();
        }

        private object lockDrive = new object();
        private void ActualizarRepositorio()
        {
            lock (lockDrive)
            {
                cache.Tabla.GuardarArchivo(pathDb);
            }
        }

        private void InicializarDb()
        {
            // Si no existe creo el archivo nada mas
            if (File.Exists(pathDb) == false)
            {
                if (Directory.Exists(DireccionDbClaveValor) == false)
                    Directory.CreateDirectory(DireccionDbClaveValor);

                ActualizarRepositorio();
                return;
            }

            // Si existe levanto todo al cache
            var datos = (Hashtable)ayArchivos.Abrir(pathDb, typeof(Hashtable));

            cache = new CacheClaveValor(datos);
        }

        public void Dispose()
        {
            cache.Dispose();
        }
    }
}
