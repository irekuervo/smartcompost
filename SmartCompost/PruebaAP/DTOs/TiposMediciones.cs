namespace PruebaAP
{
    public enum TiposMediciones
    {
        Humedad,
        Bateria,
        Temperatura
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
                default:
                    return "gen";
            }
        }
    }
}

