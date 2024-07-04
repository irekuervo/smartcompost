using nanoFramework.Json;
using nanoFramework.Networking;
using NanoKernel.Loggin;
using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace NanoKernel.Ayudantes
{
    public static class ayInternet
    {
        public static bool HayInternet(int timeout = 1500)
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
            catch (Exception)
            {
                return false;
            }
        }

        public static bool ConectarsePorWifi(string ssid, string password)
        {
            CancellationTokenSource cs = new(10_000);

            var conectado = WifiNetworkHelper.ConnectDhcp(ssid, password, token: cs.Token);
            if (conectado == false)
            {
                return false;
            }

            return ayInternet.HayInternet();
        }

        public static MacAddress GetMacAddress()
        {
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
            if (nis.Length == 0)
                return null;

            return new MacAddress(nis[0].PhysicalAddress);
        }

        public static string DoPost(string endpointURL, object objeto)
        {
            using (HttpClient client = new HttpClient())
            {
                string jsonPayload = aySerializacion.ToJson(objeto);
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using (HttpResponseMessage response = client.Post(endpointURL, content))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return response.Content.ReadAsString();
                    }
                    else
                    {
                        string error = $"Error al enviar la solicitud. Código de estado: {response.StatusCode}";
                        Logger.Log(error);
                        return error;
                    }
                }
            }
        }

        public static IPAddress[] ObtenerTodasLasIpLocalesV4()
        {
            const string hostLocal = ""; // lo dice la documentacion de  Dns.GetHostEntry("");
            IPHostEntry ip = Dns.GetHostEntry(hostLocal);
            return ip.AddressList;
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
