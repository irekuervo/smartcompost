using System;

namespace NanoKernel.Ayudantes
{
    public static class ayUnidades
    {

        public static string FormatearBytes(this int bytes, int decimales = 2)
        {
            return FormatearMagnitud(bytes, decimales, "B");
        }
        public static string FormatearBytes(this long bytes, int decimales = 2)
        {
            return FormatearMagnitud(bytes, decimales, "B");
        }
        public static string FormatearBytes(this double bytes, int decimales = 2)
        {
            return FormatearMagnitud(bytes, decimales, "B");
        }

        public static string FormatearTemperatura(this double temperatura, int decimales = 2)
        {
            return FormatearMagnitud(temperatura, decimales, "ºC");
        }

        public static string FormatearPotencia(this double tension, int decimales = 2)
        {
            return FormatearMagnitud(tension, decimales, "W");
        }

        public static string Formatear_dBm(this double valor, int decimales = 2)
        {
            return FormatearMagnitud(valor, decimales, "dBm");
        }

        public static string FormatearTension(this double tension, int decimales = 2)
        {
            return FormatearMagnitud(tension, decimales, "V");
        }

        public static string FormatearGanancia(this double ganancia, int decimales = 2)
        {
            return FormatearMagnitud(ganancia, decimales, "dB");
        }

        public static string FormatearFrecuencia(this double Frequency, int decimales = 2)
        {
            return FormatearMagnitud(Frequency, decimales, "Hz");
        }

        public static string FormatearAngulo(this double valor, int decimales = 2)
        {
            return FormatearMagnitud(valor, decimales, "°");
        }


        /// FORMATEAR MAGNITUD

        public static string ToMagnitudStr(this uint valor, int decimales = 3, string unidad = null, bool abreviado = false)
        {
            return FormatearMagnitud(valor, decimales, unidad, abreviado);
        }

        public static string ToMagnitudStr(this float valor, int decimales = 3, string unidad = null, bool abreviado = false)
        {
            return FormatearMagnitud(valor, decimales, unidad, abreviado);
        }

        public static string ToMagnitudStr(this double valor, int decimales = 3, string unidad = null, bool abreviado = false)
        {
            return FormatearMagnitud(valor, decimales, unidad, abreviado);
        }

        public static string ToMagnitudStr(this long valor, int decimales = 3, string unidad = null, bool abreviado = false)
        {
            return FormatearMagnitud(valor, decimales, unidad, abreviado);
        }

        public static string MilisToTiempo(this int milisegundos)
        {
            return MilisToTiempo((long)milisegundos);
        }

        public static string MilisToTiempo(this float milisegundos)
        {
            return MilisToTiempo((long)milisegundos);
        }

        public static string MilisToTiempo(this long milisegundos)
        {
            if (milisegundos < 1000)
            {
                return $"{milisegundos.ToMagnitudStr()} ms";
            }
            else if (milisegundos < 60000)
            {
                double segundos = milisegundos / 1000.0;
                return $"{segundos.ToMagnitudStr()} s";
            }
            else if (milisegundos < 3600000)
            {
                double minutos = milisegundos / 60000.0;
                return $"{minutos.ToMagnitudStr()} min";
            }
            else if (milisegundos < 86400000)
            {
                double horas = milisegundos / 3600000.0;
                return $"{horas.ToMagnitudStr()} hs";
            }
            else if (milisegundos < 31536000000)
            {
                double dias = milisegundos / 86400000.0;
                return $"{dias.ToMagnitudStr()} días";
            }
            else
            {
                double anios = milisegundos / 31536000000.0;
                return $"{anios.ToMagnitudStr()} años";
            }
        }

        public static string FormatearMagnitud(double valor, int decimales = 3, string unidad = null, bool abreviado = true)
        {
            if (valor == 0 && unidad != null) return FormatoString(0, null, unidad);

            if (valor == double.NaN) return "NaN";

            if (valor == double.PositiveInfinity) return "(+)∞";

            if (valor == double.NegativeInfinity) return "(-)∞";

            string[] incPrefixes;
            string[] decPrefixes;

            if (abreviado)
            {
                incPrefixes = new[] { "k", "M", "G", "T", "P", "E", "Z", "Y" };
                decPrefixes = new[] { "m", "\u03bc", "n", "p", "f", "a", "z", "y" };
            }
            else
            {
                incPrefixes = new[] { "kilo", "mega", "giga", "tera", "peta", "exa", "zetta", "yotta" };
                decPrefixes = new[] { "mili", "micro", "nano", "pico", "femto", "atto", "zepto", "yocto" };
            }

            int degree = (int)Math.Floor(Math.Log10(Math.Abs(valor)) / 3);
            double scaled = valor * Math.Pow(1000, -degree);

            string prefix;
            if (degree == 0)
                prefix = null;
            else
            {
                /// Si esta fuera de rango no le pongo unidad
                if (degree > incPrefixes.Length || degree < -incPrefixes.Length)
                    return valor.ToString();

                prefix = Math.Sign(degree) == 1 ? incPrefixes[degree - 1].ToString() : decPrefixes[-degree - 1].ToString();
            }


            return FormatoString(Math.Round(scaled* decimales) / decimales, prefix, unidad);
        }

        private static string FormatoString(double num, string prefix = null, string unidad = null)
        {
            if (prefix == null && unidad == null) return num.ToString();

            return $"{num.ToString()} {prefix}{unidad}";
        }
    }
}
