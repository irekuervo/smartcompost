﻿namespace NanoKernel.Ayudantes
{
    public static class ayArrays
    {
        public static bool IsEqualsTo(this byte[] array1, byte[] array2)
        {
            if (array1 == array2)
                return true;

            if (array1 == null || array2 == null || array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                    return false;
            }

            return true;
        }
    }
}
