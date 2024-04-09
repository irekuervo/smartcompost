using nanoFramework.Json;
using System.Text;

namespace System
{
    public static class ayString
    {
        public static string Replace(this string text, string oldValue, string newValue)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(text);
            sb.Replace(oldValue, newValue);
            return sb.ToString();
        }

        public static object Convertir(this string value, Type type)
        {
            if (type == typeof(byte))
                return Convert.ToByte(value);
            else if (type == typeof(short))
                return Convert.ToInt16(value);
            else if (type == typeof(ushort))
                return Convert.ToUInt16(value);
            else if (type == typeof(int))
                return Convert.ToInt32(value);
            else if (type == typeof(uint))
                return Convert.ToUInt32(value);
            else if (type == typeof(long))
                return Convert.ToInt64(value);
            else if (type == typeof(ulong))
                return Convert.ToUInt64(value);
            else if (type == typeof(float))
                return Convert.ToSingle(value);
            else if (type == typeof(double))
                return Convert.ToDouble(value);
            else if (type == typeof(bool))
                return value.ToLower() == bool.TrueString.ToLower() ? true : false;
            else if (type.IsClass)
                return JsonConvert.DeserializeObject(value, type);

            throw new Exception("Tipo no soportado: " + type.Name);
        }

        public static bool EquivaleA(this string a, string b)
        {
            bool aVacio = a.IsNullOrWhiteSpace();
            bool bVacio = b.IsNullOrWhiteSpace();
            if (aVacio && bVacio) return true;
            if (aVacio || bVacio) return false;

            return string.Equals(a.Trim(), b.Trim());
        }

        #region IsNull / White
        public static bool IsNull(this string s)
        {
            return s == null;
        }

        public static bool IsNotWhite(this string s)
        {
            return !IsNullOrWhite(s);
        }

        public static bool IsWhite(this string s)
        {
            return IsNullOrWhite(s);
        }

        public static bool IsNullOrWhite(this string s)
        {
            return IsNullOrWhiteSpace(s);
        }

        private static bool IsNullOrWhiteSpace(this string value)
        {
            if (value == null) return true;

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == ' ') return false;
            }

            return true;
        }
        #endregion
    }
}
