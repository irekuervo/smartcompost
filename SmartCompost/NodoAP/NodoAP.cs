using Equipos.SX127X;
using NanoKernel.Ayudantes;
using NanoKernel.Dominio;
using NanoKernel.DTOs;
using NanoKernel.Hilos;
using NanoKernel.Logging;
using NanoKernel.Nodos;
using System;
using System.Device.Gpio;
using System.Threading;

namespace NodoAP
{
    public class NodoAP : NodoBase
    {
        public override TiposNodo tipoNodo => TiposNodo.AccessPointLora;

        private const string WIFI_SSID = "Bondiola 2.4";//"SmartCompost";
        private const string WIFI_PASS = "conpapafritas";//"Quericocompost";

        private const string SMARTCOMPOST_HOST = "192.168.1.6";//"181.88.245.34";
        private string URLaddMeasurments = $"http://{SMARTCOMPOST_HOST}:8080/api/ap/{{0}}/measurements";
        private string URLkeepAlive = $"http://{SMARTCOMPOST_HOST}:8080/api/nodes/{{0}}/alive";

        private const int milisLoopColaMensajes = 5000;
        private int secondsKeepAlive = 60;

        private ApMedicionesDto apMediciones = new ApMedicionesDto();
        private ConcurrentQueue colaMediciones = new ConcurrentQueue(50);
        private object lockMensaje = new object();
        private Hilo hiloMensajes;
        private GpioController gpio;
        private GpioPin led;
        private LoRaDevice lora;
        private DateTime ultimoRequest = DateTime.MinValue;

        public override void Setup()
        {
            // Configuramos el LED
            gpio = new GpioController();
            led = gpio.OpenPin(2, PinMode.Output);
            // Prendemos el led para avisar que estamos configurando
            led.Write(PinValue.High);

            // Conectamos a internet
            Hilo.Intentar(() =>
            {
                ayInternet.ConectarsePorWifi(WIFI_SSID, WIFI_PASS);

                string ip = ayInternet.ObtenerIp();
                if (ip == "0.0.0.0")
                    throw new Exception("No se pudo asignar la ip");

                // IP asignada
                Logger.Log(ip);
            }, $"Wifi: {WIFI_SSID}-{WIFI_PASS}");

            // Vemos si podemos pingear la api
            bool ping = ayInternet.Ping(SMARTCOMPOST_HOST);
            if (ping == false)
                Logger.Log("NO PUEDO LLEGAR AL SERVIDOR");

            // Levantamos el hilo de mensajes
            hiloMensajes = MotorDeHilos.CrearHiloLoop("EnvioMensajes", LoopColaMensajes);
            hiloMensajes.Iniciar();

            // Configuramos el Lora
            Hilo.Intentar(() =>
            {
                lora = new LoRaDevice();
                lora.Iniciar();
            }, "Lora");
            lora.OnReceive += Device_OnReceive;

            // Avisamos que terminamos de configurar
            led.Write(PinValue.Low);
        }

        public override void Loop(ref bool activo)
        {
            try
            {
                if ((DateTime.UtcNow - ultimoRequest).TotalSeconds > secondsKeepAlive)
                    DoPost(CrearUrl(URLkeepAlive, NumeroSerie));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            Thread.Sleep(1000);
        }

        private void Device_OnReceive(object sender, SX127XDevice.OnDataReceivedEventArgs e)
        {
            try
            {
                Logger.Debug($"PacketSNR: {e.PacketSnr}, PacketRSSI: {e.PacketRssi}dBm, RSSI: {e.Rssi}dBm, Length: {e.Data.Length}bytes");

                lock (lockMensaje)
                {
                    if (colaMediciones.Enqueue(e.Data) != null)
                        Logger.Debug("Se perdieron mediciones");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        private void LoopColaMensajes(ref bool activo)
        {
            try
            {
                if (colaMediciones.IsEmpty())
                    return;

                apMediciones.nodes_measurements.Clear();

                lock (lockMensaje)
                {
                    foreach (var item in colaMediciones.GetItems())
                    {
                        apMediciones.AgregarMediciones((byte[])item);
                    }

                    colaMediciones.Clear();
                }

                apMediciones.last_updated = DateTime.UtcNow;
                Hilo.Intentar(
                    () => DoPost(CrearUrl(URLaddMeasurments, this.NumeroSerie), apMediciones),
                    nombreIntento: "Envio mediciones",
                    milisIntento: 1000,
                    intentos: 3);

                apMediciones.nodes_measurements.Clear();

                Blink(100);
                Logger.Debug($"Mediciones enviadas");
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
            finally
            {
                Thread.Sleep(milisLoopColaMensajes);
            }
        }

        private string CrearUrl(string url, params object[] parameters)
        {
            return string.Format(url, parameters);
        }

        private void DoPost(string url, object payload = null)
        {
            ayInternet.DoPost(url, payload);
            ultimoRequest = DateTime.UtcNow;
        }

        private void Blink(int time)
        {
#if DEBUG
            led.Write(PinValue.High);
            Thread.Sleep(time);
            led.Write(PinValue.Low);
#endif
        }
    }
}
