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
        public static bool Hay = false;
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
                    Hay = true;
                    return true;
                }
            }
            catch (Exception)
            {
                Hay = false;
                return false;
            }
        }

        public static bool ConectarsePorWifi(string ssid, string password)
        {
            CancellationTokenSource cs = new(10_000);

            var conectado = WifiNetworkHelper.ConnectDhcp(ssid, password, token: cs.Token);

            if (conectado == false)
            {
                Hay = false;
                return false;
            }

            return ayInternet.HayInternet();
        }

        /// <summary>
        /// Return MAC Address from network interface.
        /// </summary>
        /// <returns>String from "First" Converted Physical Address</returns>
        /// <remarks>Usage: string mac = Utilities.GetMacId();</remarks>
        public static MacAddress GetMacAddress()
        {
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
            if (nis.Length == 0)
                return null;

            return new MacAddress(nis[0].PhysicalAddress);
        }

        public static string EnviarJson(string endpointURL, object objeto)
        {
            using (HttpClient client = new HttpClient())
            {
                string jsonPayload = JsonSerializer.SerializeObject(objeto);
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response = client.Post(endpointURL, content);

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
                catch (Exception ex)
                {
                    string error = $"Error al enviar la solicitud: {ex.Message}";
                    Logger.Log(error);
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
    }

    public class MacAddress {

        public byte[] Address => mac;

        private static byte[] zero= { 0,0,0,0,0,0 };

        private readonly byte[] mac;

        public MacAddress(byte[] mac)
        {
            if (mac == null || mac.Length != 6 || mac.IsEqualsTo(zero))
                throw new Exception("Invalid mac address");

            this.mac = mac;
        }
    }
}
