using NanoKernel.Ayudantes;
using NanoKernel.DTOs;
using NanoKernel.Logging;
using System;
using System.Net.Http;
using System.Text;

namespace NanoKernel.Herramientas.Comunicacion
{
    public class SmartCompostClient
    {
        public DateTime UltimoRequest => ultimoRequest;

        // API CONTROLLERS
        private const string AP_Api = "ap/";
        private const string NodesApi = "nodes/";

        // POST METHODS
        private const string POST_addMeasurmentsAP = AP_Api + "{0}/measurements";
        private const string POST_addMeasurments = NodesApi + "{0}/measurements";

        // Readonly Variables
        private readonly string baseUrl;
        private readonly HttpClient client;

        // Variables
        private DateTime ultimoRequest = DateTime.MinValue;
        public SmartCompostClient(string host, string port, int timeoutSeconds = 15)
        {
            this.client = new HttpClient();
            this.client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            this.baseUrl = string.Format("http://{0}:{1}/api/", host, port);
        }

        public void AddNodeMeasurments(string apSerialNumber, MedicionesNodoDto medicionesApDto)
        {
            DoPost(POST_addMeasurmentsAP, medicionesApDto.ToJson(), apSerialNumber);
        }

        public void AddApMeasurments(string apSerialNumber, MedicionesApDto medicionesApDto)
        {
            DoPost(POST_addMeasurmentsAP, medicionesApDto.ToJson(), apSerialNumber);
        }

        private void DoPost(string method, string jsonBody, params string[] methodParams)
        {
            jsonBody = jsonBody ?? "{}";
            method = methodParams == null ? method : string.Format(method, methodParams);
            string url = baseUrl + method;

            Logger.Debug($"Post: {url} [{jsonBody.Length}]bytes\r\n \t{jsonBody}");

            try
            {
                using (StringContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json"))
                using (HttpResponseMessage response = client.Post(url, content))
                {
                    ultimoRequest = DateTime.UtcNow;
                    if (response.IsSuccessStatusCode == false)
                    {
                        string error = $"Error al enviar la solicitud. Código de estado: {response.StatusCode}";
                        Logger.Error(error);
                    }
                }
            }
            finally
            {
                jsonBody = null;
            }
        }
    }
}
