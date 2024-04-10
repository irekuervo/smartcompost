using nanoFramework.Networking;
using System;
using System.Device.Wifi;
using System.Diagnostics;
using System.Net;
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

        public static IPAddress[] ObtenerTodasLasIpLocalesV4()
        {
            const string hostLocal = ""; // lo dice la documentacion de  Dns.GetHostEntry("");
            IPHostEntry ip = Dns.GetHostEntry(hostLocal);
            return ip.AddressList;
        }
    }
}
