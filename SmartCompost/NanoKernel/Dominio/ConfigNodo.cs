using System;

namespace NanoKernel.Dominio
{
    public class ConfigNodo
    {
        public const string NombreArchivo = "configNodo.json";

        private const string DEFAULT_SSID = "SmartCompost"; //"Bondiola 2.4";
        private const string DEFAULT_PASSWORD = "Quericocompost"; //"conpapafritas";
        private const string SMARTCOMPOST_HOST = "181.88.245.34"; //"192.168.1.6";
        private const string SMARTCOMPOST_PORT = "8080";
        private const string NUMERO_SERIE_DEFAULT = "0";

        public DateTime FechaCompilacion { get; set; }
        public string HostCompilacion { get; set; }
        public string CommitHash { get; set; }
        public string NumeroSerie { get; set; }
        public string RouterSSID { get; set; }
        public string RouterPassword { get; set; }
        public string SmartCompostHost { get; set; }
        public string SmartCompostPort { get; set; }

        public override string ToString()
        {
            return $"NumeroSerie: {NumeroSerie} \r\n " +
                $"ComitHash: {CommitHash} \r\n" +
                $"HostCompilacion: {HostCompilacion} \r\n" +
                $"FechaCompilacion: {FechaCompilacion} \r\n" +
                $"HostCompilacion: {HostCompilacion}:{SmartCompostPort}\r\n";
        }

        public static ConfigNodo Default()
        {
            return new ConfigNodo()
            {
                NumeroSerie = NUMERO_SERIE_DEFAULT,
                RouterSSID = DEFAULT_SSID,
                RouterPassword = DEFAULT_PASSWORD,
                SmartCompostHost = SMARTCOMPOST_HOST,
                SmartCompostPort = SMARTCOMPOST_PORT
            };
        }
    }
}
