using Equipos.SX127X;
using NanoKernel.Ayudantes;
using NanoKernel.Dominio;
using NanoKernel.Hilos;
using NanoKernel.Logging;
using NanoKernel.Nodos;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;

namespace PruebaAP
{
    public class NodoAP : NodoBase
    {
        public override TiposNodo tipoNodo => TiposNodo.AccessPointLora;

        private ConcurrentQueue colaMensajes = new ConcurrentQueue(50);
        private Hilo hiloMensajes;

        private GpioController gpio;
        private GpioPin led;
        private LoRaDevice lora;
        private uint paquete = 1;

        private const string WIFI_SSID = "SmartCompost";//"Bondiola 2.4";
        private const string WIFI_PASS = "Quericocompost";//"comandante123";

        private const string CLOUD_HOST = "181.88.245.34";

        public override void Setup()
        {
            Logger.Log("----ACCESS POINT----");
            Logger.Log("");

            // Configuramos el LED
            gpio = new GpioController();
            led = gpio.OpenPin(2, PinMode.Output);
            // Prendemos el led para avisar que estamos configurando
            led.Write(PinValue.High);

            //// Configuramos el Lora
            lora = new LoRaDevice();
            lora.OnReceive += Device_OnReceive;
            lora.OnTransmit += Device_OnTransmit;
            // Intentamos conectarnos al lora
            Hilo.Intentar(() => lora.Iniciar(), "Lora");

            // Conectamos a internet
            Hilo.Intentar(() => ayInternet.ConectarsePorWifi(WIFI_SSID, WIFI_PASS), $"Wifi: {WIFI_SSID}-{WIFI_PASS}");
            Logger.Log(ayInternet.ObtenerIp());
            bool ping = ayInternet.Ping(CLOUD_HOST);
            Debug.Assert(ping);

            hiloMensajes = MotorDeHilos.CrearHiloLoop("EnvioMensajes", LoopMensajes);
            hiloMensajes.Iniciar();

            // Avisamos que terminamos de configurar
            led.Write(PinValue.Low);
        }

        public override void Loop(ref bool activo)
        {
            Thread.Sleep(Timeout.Infinite); // Liberamos el thread, no necesitamos el loop
        }

        private void Device_OnReceive(object sender, SX127XDevice.OnDataReceivedEventArgs e)
        {
            try
            {
                if (colaMensajes.Enqueue(e.Data) != null)
                    Logger.Error("Cola desbordada!!!");

                Logger.Log($"PacketSNR: {e.PacketSnr}, Packet RSSI: {e.PacketRssi}dBm, RSSI: {e.Rssi}dBm, Length: {e.Data.Length}bytes");

                Blink(100);
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

        private void Device_OnTransmit(object sender, SX127XDevice.OnDataTransmitedEventArgs e)
        {
            Logger.Log("Se envio el paquete " + paquete);
        }

        private Random rnd = new Random();
        private string url = $"http://{CLOUD_HOST}:8080/api/nodes/1/measurements";
        private void LoopMensajes(ref bool activo)
        {
            try
            {
                if (colaMensajes.IsEmpty())
                {
                    Logger.Log("Cola vacia");
                    return;
                }

                var m = new MensajeMediciones();
                m.last_updated = DateTime.UtcNow;

                while (colaMensajes.IsEmpty() == false)
                {
                    byte[] mensaje = (byte[])colaMensajes.Dequeue();
                    m.node_measurements.Add(new Medicion()
                    {
                        timestamp = DateTime.UtcNow,
                        type = "temp",
                        value = (float)(rnd.NextDouble() * 5 + 25)
                    });
                }

                try
                {
                    ayInternet.DoPost(url, m);
                    Logger.Log($"Se enviaron {m.node_measurements.Count} mediciones");
                    Logger.Log($"Quedan {colaMensajes.Count()} en la cola");
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                    // aca deberia poder reencolar todo denuevo
                    // colaMensajes.Enqueue(mensaje);
                }
            }
            finally
            {
                Thread.Sleep(5000);
            }
        }

    }
}
