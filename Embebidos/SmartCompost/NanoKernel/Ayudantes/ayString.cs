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
    }
}
