using nanoFramework.Json;
using System.Collections;
using System.IO;
using System.Text;

namespace NanoKernel.Repositorios
{


    public class BaseDeDatosDocumental
    {
        public Hashtable Registros { get; set; } = new Hashtable();
    }

    public class RepositorioDocumentalInterno : IRepositorioDocumental
    {
        const string unidadBase = @"I:\";


        private string pathDb;

        public RepositorioDocumentalInterno(string direccion)
        {
            this.pathDb = unidadBase + direccion;
            //if (File.Exists(pathDb) == false)
            {
                EscribirDb(new BaseDeDatosDocumental());
            }
        }

        public void Update(string id, string value)
        {
            var db = ObtenerDb();

            if (db.Registros == null)
                db.Registros = new Hashtable();

            db.Registros.Add(id, value);

            EscribirDb(db);
        }

        public void Delete(string id)
        {
            //File.Delete(ObtenerPath(id));
        }

        public string Get(string id)
        {
            var db = ObtenerDb();

            if (db.Registros.Contains(id) == false)
                return null;

            return db.Registros[id] as string;
        }

        public BaseDeDatosDocumental ObtenerDb()
        {
            if (!File.Exists(pathDb))
                return null;

            using (FileStream fs2 = new FileStream(pathDb, FileMode.Open, FileAccess.Read))
            {
                byte[] fileContent = new byte[fs2.Length];
                fs2.Read(fileContent, 0, (int)fs2.Length);
                return (BaseDeDatosDocumental)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(fileContent, 0, fileContent.Length), typeof(BaseDeDatosDocumental));
            }
        }

        private void EscribirDb(BaseDeDatosDocumental db)
        {
            if (!File.Exists(pathDb))
                return;

            File.Create(pathDb);

            byte[] sampleBuffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(db));
            using (FileStream fs = new FileStream(pathDb, FileMode.Open, FileAccess.ReadWrite))
            {
                fs.Write(sampleBuffer, 0, sampleBuffer.Length);
            }
        }

    }
}
