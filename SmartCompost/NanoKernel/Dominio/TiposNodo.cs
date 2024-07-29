namespace NanoKernel.Dominio
{
    public enum TiposNodo
    {
        Generico = 0,
        AccessPointLora,
        MedidorLora
    }

    public static class ayTiposNodo
    {
        public static string GetDescripcion(this TiposNodo tipo)
        {
            switch (tipo)
            {
                case TiposNodo.Generico:
                    return "Generico";
                case TiposNodo.AccessPointLora:
                    return "Access Point Lora";
                case TiposNodo.MedidorLora:
                    return "Medidor Lora";
                default:
                    return "Sin definir";
            }
        }
    }
}
