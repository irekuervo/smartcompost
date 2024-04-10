using nanoFramework.Json;
using System.Collections;
using System.IO;
using System.Text;

namespace NanoKernel.Repositorios
{
    /// <summary>
    /// Herramienta para manejar un repo en la unidad I: propia del micro
    /// Esto usa la memoria EEPROM, NO ADMITE ACTUALMENTE DIRECTORIOS (podria implementarse un mecanismo)
    /// </summary>
    public class RepositorioClaveValorInterno : IRepositorioClaveValor
    {
        private const string unidadBase = @"I:\";

        private string pathDb;

        private CacheClaveValor cache = new CacheClaveValor();

        public RepositorioClaveValorInterno(string direccion)
        {
            this.pathDb = unidadBase + direccion;

            Inicializar();
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
                if (File.Exists(pathDb) == false)
                    throw new System.Exception();

                byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(cache.Tabla));

                using (FileStream fs = new FileStream(pathDb, FileMode.Open, FileAccess.ReadWrite))
                {
                    fs.Write(buffer, 0, buffer.Length);
                }
            }
        }

        private void Inicializar()
        {
            if (File.Exists(pathDb) == false)
            { 
                File.Create(pathDb).Close();
                ActualizarRepositorio();
                return;
            }
            
            byte[] fileContent;
            using (FileStream fs2 = new FileStream(pathDb, FileMode.Open, FileAccess.Read))
            {
                fileContent = new byte[fs2.Length];
                fs2.Read(fileContent, 0, (int)fs2.Length);
            }

            var datos = (Hashtable)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(fileContent, 0, fileContent.Length), typeof(Hashtable));
            cache = new CacheClaveValor(datos);
        }
    }
}
