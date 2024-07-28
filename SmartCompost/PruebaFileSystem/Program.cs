using NanoKernel.Ayudantes;
using NanoKernel.Herramientas.Repositorios;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace PruebaFileSystem
{
    public class Program
    {
        private const string unidadBase = @"I:\";
        public static void Main()
        {
            Console.WriteLine("Drives system:");
            foreach (var item in DriveInfo.GetDrives())
            {
                Console.WriteLine($"{item.Name} type: {item.DriveType} size: {item.TotalSize}bytes");
            }

            // Creo un archivo
            string path = unidadBase + "texto.txt";
            byte[] buffer = Encoding.UTF8.GetBytes("hola");
            using (FileStream fs = File.Create(path))
            {
                fs.Write(buffer, 0, buffer.Length);
            }

            // Leo un archivo
            using (StreamReader sr = File.OpenText(path))
            {
                Console.WriteLine(sr.ReadToEnd());
            }

            // Creo una carpeta (ESTO ES NUEVO!!)
            string dir = unidadBase + @"carpeta\";
            Directory.CreateDirectory(dir);
            foreach (var item in Directory.GetDirectories(unidadBase))
            {
                Console.WriteLine(item);
            }

            Console.WriteLine(Directory.Exists(dir).ToString());

            // Como puedo crear carpetas, puedo tener repos en carpetas!!
            using (var db = new RepositorioClaveValorInterno("dbTest"))
            {
                const string clave = "clave";
                db.Update(clave, "valor en la clave");
                Console.WriteLine(db.Get(clave));
            }

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
