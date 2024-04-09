using nanoFramework.Networking;
using System;
using System.Device.Wifi;
using System.Net;
using System.Threading;

namespace NanoKernel.Ayudantes
{
    public class ayInternet
    {
        public static bool HayInternet(int timeout = 1500)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://google.com/generate_204");

                request.Timeout = timeout;
                request.ReadWriteTimeout = timeout;

                var response = (HttpWebResponse)request.GetResponse();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ConectarsePorWifi(string ssid, string password, WifiReconnectionKind reconnectionKind = WifiReconnectionKind.Automatic, bool requiresDateTime = false, int wifiAdapterId = 0, CancellationToken token = default(CancellationToken))
        {
            return WifiNetworkHelper.ConnectDhcp(ssid, password, reconnectionKind, requiresDateTime, wifiAdapterId, token);
        }

        public static IPAddress[] ObtenerTodasLasIpLocalesV4()
        {
            const string hostLocal = ""; // lo dice la documentacion de  Dns.GetHostEntry("");
            IPHostEntry ip = Dns.GetHostEntry(hostLocal);
            return ip.AddressList;
        }
    }
}
