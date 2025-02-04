namespace NanoKernel.Ayudantes
{
    public static class ayNumeros
    {
        public delegate bool AnyDelegate(uint comparador);

        public static bool Any(this uint[] list, AnyDelegate d)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (d.Invoke(list[i]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Acota los valores entre 0 y 100, establece una cota inferior, y permite una escala entre valores.
        /// </summary>
        /// <param name="porcentaje"></param>
        /// <param name="minTresh">puedo elegir la cota minima de porcentaje</param>
        /// <param name="escala">puedo elegir el tamaño del salto entre portentajes</param>
        /// <returns></returns>
        public static float ToPorcentaje(this float porcentaje, int minTresh = 0, int escala = 0)
        {
            porcentaje = porcentaje < minTresh
                  ? minTresh
                  : (porcentaje > 100 ? 100 : porcentaje);

            escala = escala < 0 ? 0 : (escala > 50 ? 50 : escala);
            if (escala == 0)
                return porcentaje;

            return (int)(porcentaje / escala) * 10;
        }
    }
}
