using System.IO;

namespace NanoKernel.Ayudantes
{
    public static class ayArchivos
    {
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
    }
}
