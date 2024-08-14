using nanoFramework.Hardware.Esp32;
using NanoKernel.Ayudantes;
using NanoKernel.Herramientas.Medidores;
using NanoKernel.Hilos;
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

        static string RouterSSID = "SmartCompost"; //"Bondiola 2.4";
        static string RouterPassword = "Quericocompost"; //"conpapafritas";
        static string SmartCompostHost = "smartcompost.net"; //"192.168.1.6";
        static string SmartCompostPort = "8080";

        static string jsonPayload;

        static HttpClient _httpClient;

        static ConcurrentQueue cola;

        const int sleepTime = 5000;

        static Medidor medidor = new Medidor(sleepTime);

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

            string idAp = "b2c40a98-5534-11ef-92ae-0242ac140004";
            string url = $"http://{SmartCompostHost}:{SmartCompostPort}/api/ap/{idAp}/measurements";
            _httpClient = new HttpClient();

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
                    MedicionesApDto dto = MedicionesApDto.Demo(nodos: 1, mediciones: 1);
                    jsonPayload = dto.ToJson();

                    Console.WriteLine(jsonPayload);

                    GetMemory("before");

                    medidor.Medir("tasa-bytes", jsonPayload.Length);

                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    try
                    {
                        var result = _httpClient.Post(url, content);
                        Console.WriteLine($"Result {result.IsSuccessStatusCode}");
                        Console.WriteLine(result.Content.ReadAsString());
                        result.EnsureSuccessStatusCode();
                        result.Dispose();
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
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
