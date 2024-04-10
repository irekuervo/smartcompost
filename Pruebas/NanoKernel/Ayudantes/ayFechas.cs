using System;
using System.Net;
using System.Net.Sockets;

namespace NanoKernel.Ayudantes
{
    public static class ayFechas
    {
        public static string FormatoFechaLocal = "yyyy-MM-dd HH:mm:ss";
        public const string NTP_Server = "ar.pool.ntp.org";

        public static DateTime Ahora => ultimaFecha.AddMilliseconds((ultimaActualizacion - DateTime.UtcNow).TotalMilliseconds);

        private static DateTime ultimaFecha = DateTime.FromUnixTimeSeconds(0);

        public static string ToFechaLocal(this DateTime dt)
        {
            return dt.ToString(FormatoFechaLocal);
        }

        private static DateTime ultimaActualizacion = DateTime.UtcNow;
        public static bool ActualizarFecha()
        {
            try
            {
                var fecha = GetNetworkTime();

                if (fecha <= ultimaFecha) return true;

                ultimaFecha = fecha;
                ultimaActualizacion = DateTime.UtcNow;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static DateTime GetNetworkTime(string server = NTP_Server)
        {
            using (var udpClient = new UdpClient(server, 123))
            {
                // Enviar solicitud NTP
                byte[] ntpData = new byte[48];
                ntpData[0] = 0x1B;
                udpClient.Send(ntpData, 0, ntpData.Length);

                // Recibir respuesta NTP
                IPEndPoint remoteEndPoint = null;
                int length = udpClient.Receive(ntpData, ref remoteEndPoint);

                // Analizar respuesta NTP
                ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | ntpData[43];
                ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | ntpData[47];
                ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

                // Convertir la hora de NTP a DateTime
                DateTime ntpTime = new DateTime(1900, 1, 1, 0, 0, 0).AddMilliseconds(milliseconds);

                return ntpTime;
            }
        }
    }
}
