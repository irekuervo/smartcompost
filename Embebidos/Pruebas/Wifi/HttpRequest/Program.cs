using nanoFramework.Json;
using nanoFramework.Networking;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;


namespace HttpRequest
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            const string Ssid = "SmartCompost";
            const string Password = "Quericocompost";
            const string endpointURL = "http://smartcompost.net:8080/api/compost_bins/add_measurement";

            // Give 60 seconds to the wifi join to happen
            CancellationTokenSource cs = new(60000);
            var success = WifiNetworkHelper.ScanAndConnectDhcp(Ssid, Password, token: cs.Token);
            if (!success)
            {
                //Red Light indicates no Wifi connection
                throw new Exception("Couldn't connect to the Wifi network");
            }


            Random ran = new Random();
            Medicion m = new();

            using (HttpClient client = new HttpClient())
            {
                while (true)
                {
                    m.id = 1;
                    m.datetime = new DateTime(2024, 4, 3, 10, ran.Next(60), ran.Next(60));
                    m.temperatura = ran.NextDouble() * 25 + 4;
                    m.humedad = ran.NextDouble() * 100;

                    string jsonPayload = JsonSerializer.SerializeObject(m);

                    // Convertir el payload JSON a un contenido StringContent
                    StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    try
                    {
                        // Enviar la solicitud POST al servidor
                        HttpResponseMessage response = client.Post(endpointURL, content);

                        // Verificar si la solicitud fue exitosa
                        if (response.IsSuccessStatusCode)
                        {
                            // La solicitud fue exitosa, imprimir el contenido de la respuesta
                            string responseContent = response.Content.ReadAsString();
                            Console.WriteLine("Respuesta del servidor:");
                            Console.WriteLine(responseContent);
                        }
                        else
                        {
                            // La solicitud no fue exitosa, imprimir el código de estado de la respuesta
                            Console.WriteLine($"Error al enviar la solicitud. Código de estado: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Manejar cualquier excepción que pueda ocurrir durante el envío de la solicitud
                        Console.WriteLine($"Error al enviar la solicitud: {ex.Message}");
                    }
                }
            }
        }

        public class Medicion
        {
            public int id { get; set; }
            public double temperatura { get; set; }
            public double humedad { get; set; }
            public DateTime datetime { get; set; }
        }
    }
}
