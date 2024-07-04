using devMobile.IoT.SX127xLoRaDevice;
using NanoKernel.Ayudantes;
using NanoKernel.Loggin;
using NanoKernel.LoRa;
using NanoKernel.Modulos;
using System;
using System.Collections;
using System.Threading;

namespace NanoKernel.Comunicacion
{
    /// <summary>
    /// Esta clase es para armar un nodo estrella lora contra wifi
    /// </summary>
    public class RouterLoraWifi : IDisposable
    {
        private readonly MacAddress direccionLocal;
        private readonly LoRaDevice lora;
        private readonly ModuloBlinkLed blinker;

        private const string URL = "http://smartcompost.net:8080/api/nodes/1/measurements";

        public RouterLoraWifi(LoRaDevice lora, ModuloBlinkLed blinker, MacAddress direccionLocal)
        {
            this.blinker = blinker;
            this.lora = lora;

            this.lora.OnReceive += Lora_OnReceive;
            this.lora.OnTransmit += Lora_OnTransmit;

            this.direccionLocal = direccionLocal;
        }

        private void Lora_OnTransmit(object sender, SX127XDevice.OnDataTransmitedEventArgs e)
        {

        }

        private void Lora_OnReceive(object sender, SX127XDevice.OnDataReceivedEventArgs e)
        {
            try
            {
                Paquete p = new Paquete(e.Data);

                if (p.MacDestino.Es(direccionLocal) == false)
                    return; // Se descarta

                blinker.BlinkOnce(100);

                Paquete paquete = new Paquete(e.Data);
                Logger.Log(paquete.ToString());
            }
            catch (Exception ex)
            {
                Logger.Log("No se pudo desempaquetar");
            }
        }

        private void EnviarPapota()
        {
            var ran = new Random();
            blinker.Detener();
            // PARA PROBAR:
            while (true)
            {
                try
                {
                    AddMeasurementDto addMeasurementDto = new AddMeasurementDto();
                    addMeasurementDto.last_updated = DateTime.UtcNow;
                    addMeasurementDto.node_measurements = new ArrayList();

                    MeasurementDto measurementDto = new MeasurementDto();
                    measurementDto.timestamp = DateTime.UtcNow;
                    measurementDto.type = "temperatura";
                    measurementDto.value = (float)ran.NextDouble() * 10 + 25;

                    addMeasurementDto.node_measurements.Add(measurementDto);

                    var res = ayInternet.DoPost(URL, addMeasurementDto);
                    Logger.Log("Enviado ok");
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                finally
                {
                    Thread.Sleep(5000);
                }
            }
        }


        public void Dispose()
        {

        }
    }
}
