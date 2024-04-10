using nanoFramework.Json;
using nanoFramework.Networking;
using NanoKernel.Loggin;
using System;
using System.Net;
using System.Net.Http;
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
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://neverssl.com/");

                request.Timeout = timeout;
                request.ReadWriteTimeout = timeout;

                var response = (HttpWebResponse)request.GetResponse();
                Hay = true;
                return true;
            }
            catch (Exception)
            {
                Hay = false;
                return false;
            }
        }

        public static bool ConectarsePorWifi(string ssid, string password)
        {
            CancellationTokenSource cs = new(60000);

            var conectado = WifiNetworkHelper.ConnectDhcp(ssid, password, token: cs.Token);

            if (conectado == false)
            {
                Hay = false;
                return false;
            }

            return ayInternet.HayInternet();
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
}
