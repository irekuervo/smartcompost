using nanoFramework.Networking;
using NanoKernel.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NanoKernel.Ayudantes
{
    public static class ayInternet
    {
        public static bool HayInternet(int timeout = 20000)
        {
            try
            {
                using (HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://neverssl.com/"))
                {
                    request.Timeout = timeout;
                    request.ReadWriteTimeout = timeout;

                    var response = (HttpWebResponse)request.GetResponse();
                    response.Dispose();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
        }

        public static string ObtenerIp()
        {
            // Obtén todas las interfaces de red disponibles
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var networkInterface in networkInterfaces)
            {
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    // Verifica que la interfaz esté conectada
                    if (networkInterface.IPv4Address != null)
                        return networkInterface.IPv4Address;
                }
            }

            return "No hay ip asignada";
        }

        public static bool ConectarsePorWifi(string ssid, string password)
        {
            CancellationTokenSource cs = new(20_000);

            var conectado = WifiNetworkHelper.ConnectDhcp(ssid, password, token: cs.Token);
            if (conectado == false)
            {
                return false;
            }

            Logger.Debug("Wifi Conectado");
            return ayInternet.HayInternet();
        }

        public static MacAddress GetMacAddress()
        {
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
            if (nis.Length == 0)
                return null;

            return new MacAddress(nis[0].PhysicalAddress);
        }

        public static string DoPost(string endpointURL, object payload = null)
        {
            string jsonPayload = payload == null ? "{}" : aySerializacion.ToJson(payload);
            Logger.Debug("Request a: " + endpointURL + jsonPayload);
            using (HttpClient client = new HttpClient())
            using (StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
            using (HttpResponseMessage response = client.Post(endpointURL, content))
            {
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsString();
                }
                else
                {
                    string error = $"Error al enviar la solicitud. Código de estado: {response.StatusCode}";
                    Logger.Error(error);
                    return error;
                }
            }
        }

        public static string DoGet(string endpointURL)
        {
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = client.Get(endpointURL))
            {
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsString();
                }
                else
                {
                    string error = $"Error al enviar la solicitud. Código de estado: {response.StatusCode}";
                    Logger.Error(error);
                    return error;
                }
            }
        }

        public static IPAddress[] ObtenerTodasLasIpLocalesV4()
        {
            const string hostLocal = ""; // lo dice la documentacion de  Dns.GetHostEntry("");
            IPHostEntry ip = Dns.GetHostEntry(hostLocal);
            return ip.AddressList;
        }

        private const int IcmpEcho = 8;
        private const int IcmpEchoReply = 0;
        public static bool Ping(string address)
        {
            try
            {
                byte[] buffer = CreatePingPacket();
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(address), 0);

                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp))
                {

                    socket.SendTo(buffer, buffer.Length, SocketFlags.None, endPoint);

                    byte[] receiveBuffer = new byte[256];
                    EndPoint responseEndPoint = new IPEndPoint(IPAddress.Any, 0);


                    socket.SendTimeout = 15000;
                    socket.ReceiveFrom(receiveBuffer, ref responseEndPoint);
                    if (receiveBuffer[20] == IcmpEchoReply)
                        return true;

                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static byte[] CreatePingPacket()
        {
            byte[] packet = new byte[32];
            packet[0] = IcmpEcho; // Echo Type
            packet[1] = 0; // Code
            packet[2] = 0; // Checksum
            packet[3] = 0; // Checksum
            packet[4] = 0; // Identifier (arbitrary)
            packet[5] = 1; // Identifier (arbitrary)
            packet[6] = 0; // Sequence number (arbitrary)
            packet[7] = 1; // Sequence number (arbitrary)

            // Calculate checksum
            ushort checksum = CalculateChecksum(packet);
            packet[2] = (byte)(checksum >> 8);
            packet[3] = (byte)(checksum & 0xff);

            return packet;
        }

        private static ushort CalculateChecksum(byte[] buffer)
        {
            int length = buffer.Length;
            int index = 0;
            uint sum = 0;

            while (length > 1)
            {
                sum += (uint)(buffer[index++] << 8 | buffer[index++]);
                length -= 2;
            }

            if (length > 0)
            {
                sum += (uint)(buffer[index] << 8);
            }

            while ((sum >> 16) != 0)
            {
                sum = (sum & 0xffff) + (sum >> 16);
            }

            return (ushort)~sum;
        }
    }


    public class MacAddress
    {
        public byte[] Address => mac;

        private static byte[] zero = { 0, 0, 0, 0, 0, 0 };

        private readonly byte[] mac;

        public MacAddress(byte[] mac)
        {
            if (mac == null || mac.Length != 6 || mac.IsEqualsTo(zero))
                throw new Exception("Invalid mac address");

            this.mac = mac;
        }

        public MacAddress(string macString)
        {
            // Eliminar caracteres no válidos en la dirección MAC
            macString = macString.Replace(":", "").Replace("-", "").ToUpper();

            // Validar que la cadena tenga el formato adecuado
            if (macString.Length != 12 || !IsHexadecimal(macString))
                throw new ArgumentException("Invalid MAC address format");

            // Convertir la cadena hexadecimal a bytes
            mac = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                mac[i] = Convert.ToByte(macString.Substring(i * 2, 2), 16);
            }
        }

        private bool IsHexadecimal(string input)
        {
            foreach (char c in input)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F')))
                    return false;
            }
            return true;
        }

        public bool Es(MacAddress mac) => mac.Address.IsEqualsTo(this.mac);

        public override string ToString()
        {
            return BitConverter.ToString(mac).Replace("-", ":");
        }
    }
}
