using nanoFramework.Hardware.Esp32;
using NanoKernel.Ayudantes;
using NanoKernel.DTOs;
using NanoKernel.Herramientas.Comunicacion;
using NanoKernel.Herramientas.Medidores;
using NanoKernel.Hilos;
using NanoKernel.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace PruebaWifi
{
    // Quiero probar como responde el MCU sin alocar memoria entre requests.

    // Lan, alocando dentro del lopp = alocando fuera del loop
    public class Program
    {
        static bool redLan = true;

        static string RouterSSID = "iplan - Mansilla -2.4Ghz"; // "SmartCompost"; //"Bondiola 2.4";
        static string RouterPassword = "Marmat192293."; //"conpapafritas";
        static string SmartCompostHost = "smartcompost.net"; //"192.168.1.6";
        static string SmartCompostPort = "8080";

        static string jsonPayload;

        private static SmartCompostClient cliente;

        private static HttpClient _httpClient;

        private static ConcurrentQueue cola;

        private const int sleepTime = 5000;

        private static Medidor medidor = new Medidor(sleepTime);

        public static void Main()
        {
            GetMemory("Al principio");

            if (redLan)
            {
                RouterSSID = "Bondiola 2.4";
                RouterPassword = "conpapafritas";
                SmartCompostHost = "192.168.1.6";
            }

            Hilo.Intentar(() => ayInternet.ConectarsePorWifi(ssid: RouterSSID, password: RouterPassword));

            Logger.Debug("CONECTADO A INTERNET");

            cliente = new SmartCompostClient(SmartCompostHost,SmartCompostPort);

            string idAp = "58670345-7dc4-11ef-919e-0242ac160004";

            cola = new ConcurrentQueue(100);

            var hilo = MotorDeHilos.CrearHiloLoop("lora", (ref bool activo) =>
            {
                cola.Enqueue(new byte[78]);
                Thread.Sleep(1000);
            });
            //hilo.Iniciar();

            medidor.OnMedicionesEnPeriodoCallback += Medidor_OnMedicionesEnPeriodoCallback;
            medidor.Iniciar();

            GetMemory("Antes de iniciar loop");

            while (true)
            {
                try
                {
                    //MedicionesApDto dto = MedicionesApDto.Demo(nodos: 1, mediciones: 1);
                    //jsonPayload = dto.ToJson();

                    //Console.WriteLine(jsonPayload);

                    //GetMemory("before");

                    //medidor.Medir("tasa-bytes", jsonPayload.Length);

                    //var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    //try
                    //{
                    //    cliente.AddApMeasurments(idAp, dto);

                    //    var result = _httpClient.Post(url, content);
                    //    Console.WriteLine($"Result {result.IsSuccessStatusCode}");
                    //    Console.WriteLine(result.Content.ReadAsString());
                    //    result.EnsureSuccessStatusCode();
                    //    result.Dispose();
                    //}
                    //catch (HttpRequestException ex)
                    //{
                    //    Console.WriteLine(ex.Message);
                    //}
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    GetMemory("after");
                    Thread.Sleep(sleepTime);
                }
            }

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Medidor_OnMedicionesEnPeriodoCallback(InstanteMedicion resultado)
        {
            var res = resultado.MedidoPeriodo("tasa-bytes");
            if (res == null)
                return;

            float tamanioTotal = res.Suma;
            Console.WriteLine($"Transferencia: {tamanioTotal}bytes/{sleepTime}milis");
        }

        public static void GetMemory(string message = "")
        {
            uint totalsize, totalFree, largestFreeBlock;

            NativeMemory.GetMemoryInfo(
                NativeMemory.MemoryType.Internal,
                out totalsize,
                out totalFree,
                out largestFreeBlock);

            Console.WriteLine($"{totalFree} bytes free " + message);
        }
    }
}
