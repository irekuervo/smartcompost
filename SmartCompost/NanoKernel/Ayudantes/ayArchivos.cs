using System;
using System.IO;

namespace NanoKernel.Ayudantes
{
    public static class ayArchivos
    {
        public const string DIR_INTERNO = @"I:\";
        public const string FORMATO_ARCHIVO = ".json";

        public static void GuardarJson(this object obj, string filePath)
        {
            if (filePath.EndsWith(FORMATO_ARCHIVO) == false)
                filePath += FORMATO_ARCHIVO;

            AsegurarDirectorio(filePath);

            File.WriteAllText(filePath, obj.ToJson());
        }

        public static object AbrirJson(string filePath, Type type)
        {
            if (filePath.EndsWith(FORMATO_ARCHIVO) == false)
                filePath += FORMATO_ARCHIVO;

            return File.ReadAllText(filePath).FromJson(type);
        }

        public static void AsegurarDirectorio(string filePath)
        {
            if (File.Exists(filePath) == false)
            {
                var dir = Path.GetDirectoryName(filePath);
                if (Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);
            }
        }

        public static string ObtenerDireccionValida(this string dir)
        {
            //MEJORAR, VALIDAR CARACTERES INCORRECTOS, ETC
            return dir;//  dir.TerminarEnBarra();
        }

        public static string TerminarEnBarra(this string dir)
        {
            if (dir.EndsWith("\\") == false)
            {
                dir = dir + "\\";
            }

            return dir;
        }

        public static bool ArchivoExiste(string filePath) => File.Exists(filePath);
    }
}
