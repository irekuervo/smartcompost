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

        public static float ToPorcentaje(this float porcentaje, int minTresh = 20)
        {
            porcentaje = porcentaje < minTresh
                  ? minTresh
                  : (porcentaje > 100 ? 100 : porcentaje);

            return (porcentaje / 10 * 10);
        }
    }
}
