using NanoKernel.Ayudantes;
using NanoKernel.Hilos;
using NanoKernel.Loggin;
using NanoKernel.LoRa;
using NanoKernel.Nodos;
using System;
using System.Device.Gpio;
using System.Text;
using System.Threading;

namespace PruebaAP
{
    public class NodoAP : NodoBase
    {
        public override string IdSmartCompost => "NODO!";
        public override TiposNodo tipoNodo => TiposNodo.AccessPoint;

        private GpioController gpio;
        private GpioPin led;
        private LoRaDevice lora;
        private uint paquete = 1;

        private const string WIFI_SSID = "Bondiola 2.4";
        private const string WIFI_PASS = "comandante123";

        public override void Setup()
        {
            Logger.Log("----ACCESS POINT----");
            Logger.Log("");

            // Configuramos el LED
            gpio = new GpioController();
            led = gpio.OpenPin(2, PinMode.Output);
            // Prendemos el led para avisar que estamos configurando
            led.Write(PinValue.High);

            // Configuramos el Lora
            lora = new LoRaDevice();
            lora.OnReceive += Device_OnReceive;
            lora.OnTransmit += Device_OnTransmit;
            // Intentamos conectarnos al lora
            Hilo.Intentar(() => lora.Iniciar(), "Lora");

            // Conectamos a internet
            Hilo.Intentar(() => ayInternet.ConectarsePorWifi(WIFI_SSID, WIFI_PASS), $"Wifi: {WIFI_SSID}-{WIFI_PASS}");

            // Avisamos que terminamos de configurar
            led.Write(PinValue.Low);
        }


        public override void Loop(ref bool activo)
        {
            Thread.Sleep(Timeout.Infinite); // Liberamos el thread, no necesitamos el loop
        }

        private void Device_OnReceive(object sender, devMobile.IoT.SX127xLoRaDevice.SX127XDevice.OnDataReceivedEventArgs e)
        {
            try
            {
                string messageText = UTF8Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);

                Blink(100);

                Logger.Log($"PacketSNR: {e.PacketSnr}, Packet RSSI: {e.PacketRssi}dBm, RSSI: {e.Rssi}dBm, Length: {e.Data.Length}bytes \r\n{messageText}");
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        private void Blink(int time)
        {
            led.Write(PinValue.High);
            Thread.Sleep(time);
            led.Write(PinValue.Low);
        }

        private void Device_OnTransmit(object sender, devMobile.IoT.SX127xLoRaDevice.SX127XDevice.OnDataTransmitedEventArgs e)
        {
            Logger.Log("Se envio el paquete " + paquete);
        }
    }
}
