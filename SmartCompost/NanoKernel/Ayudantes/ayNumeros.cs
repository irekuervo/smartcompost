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
    }
}
