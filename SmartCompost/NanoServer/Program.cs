using NanoKernel.Ayudantes;
using NanoKernel.Hilos;
using NanoKernel.Loggin;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace NanoServer
{
    public class Program
    {
        private const string WIFI_SSID = "SmartCompost";//"Bondiola 2.4";
        private const string WIFI_PASS = "Quericocompost";//"comandante123";

        private const string CLOUD_HOST = "181.88.245.34";


        // IP DONGLE 186.141.197.35
        public static void Main()
        {
            // Conectamos a internet
            Hilo.Intentar(() => ayInternet.ConectarsePorWifi(WIFI_SSID, WIFI_PASS), $"Wifi: {WIFI_SSID}-{WIFI_PASS}");
            Logger.Log(ayInternet.ObtenerIp());
            // bool ping = ayInternet.Ping(CLOUD_HOST);
            //Debug.Assert(ping);

            // Configurar el servidor HTTP
            var listener = new HttpListener("http", 8080);

            // Iniciar el servidor
            listener.Start();
            Debug.WriteLine("Servidor HTTP iniciado");

            // Manejar solicitudes entrantes
            while (true)
            {
                var context = listener.GetContext();
                var request = context.Request;
                var response = context.Response;

                Debug.WriteLine("Solicitud recibida");

                if (request.HttpMethod == "GET")
                {
                    // Configurar la respuesta
                    var responseString = "<html><body><h1>Hola desde nanoFramework!</h1></body></html>";
                    var buffer = Encoding.UTF8.GetBytes(responseString);

                    // Escribir la respuesta
                    response.ContentLength64 = buffer.Length;
                    var output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();

                    Debug.WriteLine("Respuesta enviada");
                }
            }
        }
    }

}
