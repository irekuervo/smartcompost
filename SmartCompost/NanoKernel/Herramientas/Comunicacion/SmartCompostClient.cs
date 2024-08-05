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
        private const string NodesApi = "nodes/";
        private const string AP_Api = "ap/";

        // POST METHODS
        private const string POST_addMeasurments = AP_Api + "{0}/measurements";
        private const string POST_keepAlive = NodesApi + "{0}/alive";
        private const string POST_startup = NodesApi + "{0}/startup";

        // Readonly Variables
        private readonly string baseUrl;
        private readonly HttpClient client;

        // Variables
        private DateTime ultimoRequest = DateTime.MinValue;
        public SmartCompostClient(string host, string port, int timeoutSeconds)
        {
            this.client = new HttpClient();
            this.client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            this.baseUrl = string.Format("http://{0}:{1}/api/", host, port);
        }

        public void AddApMeasurments(string apSerialNumber, string medicionesApDto)
        {
            DoPost(POST_addMeasurments, medicionesApDto, apSerialNumber);
        }

        public void NodeAlive(string nodeSerialNumber)
        {
            DoPost(POST_keepAlive, null, nodeSerialNumber);
        }

        public void NodeStartup(string nodeSerialNumber)
        {
            DoPost(POST_startup, null, nodeSerialNumber);
        }

        private void DoPost(string method, string jsonBody, params string[] methodParams)
        {
            jsonBody = jsonBody ?? "{}";
            method = methodParams == null ? method : string.Format(method, methodParams);
            string url = baseUrl + method;

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
    }
}
