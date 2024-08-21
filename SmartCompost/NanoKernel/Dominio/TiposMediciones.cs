namespace NanoKernel.Dominio
{
    public enum TiposMediciones
    {
        Humedad,
        Bateria,
        Temperatura,
        MensajesTirados,
        MensajesEnviados,
        TamanioCola,
        Errores,
        Startup
    }

    public static class ayTiposMediciones
    {
        public static string GetString(this TiposMediciones tipo)
        {
            switch (tipo)
            {
                case TiposMediciones.Humedad:
                    return "hum";
                case TiposMediciones.Bateria:
                    return "bat";
                case TiposMediciones.Temperatura:
                    return "temp";
                case TiposMediciones.MensajesTirados:
                    return "tir";
                case TiposMediciones.MensajesEnviados:
                    return "env";
                case TiposMediciones.TamanioCola:
                    return "cola";
                case TiposMediciones.Errores:
                    return "err";
                case TiposMediciones.Startup:
                    return "start";
                default:
                    return "gen";
            }
        }
    }
}

