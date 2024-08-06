using nanoFramework.Json;
using System;

namespace PruebaAP
{
    public static class aySerializacion
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static object FromJson(this string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }

        public static object FromString(Type tipoParametro, string text)
        {
            return text.Convertir(tipoParametro);
        }
    }
}
