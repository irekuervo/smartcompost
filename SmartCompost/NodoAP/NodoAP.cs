using Equipos.SX127X;
using NanoKernel.Ayudantes;
using NanoKernel.Comunicacion;
using NanoKernel.Dominio;
using NanoKernel.Hilos;
using NanoKernel.Logging;
using NanoKernel.Nodos;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace NodoAP
{
    public class NodoAP : NodoBase
    {
        public override string IdSmartCompost => "NODO AP TEST";
        public override TiposNodo tipoNodo => TiposNodo.AccessPoint;

        private ConcurrentQueue colaMensajes = new ConcurrentQueue(50);
        private Hilo hiloMensajes;

        private GpioController gpio;
        private GpioPin led;
        private LoRaDevice lora;

        private const string WIFI_SSID = "Bondiola 2.4";//"SmartCompost";
        private const string WIFI_PASS = "conpapafritas";//"Quericocompost";

        private const string CLOUD_HOST = "192.168.1.6";//"181.88.245.34";
        private string URLaddMeasurments = $"http://{CLOUD_HOST}:8080/api/nodes/{{0}}/measurements";
        private string URLkeepAlive = $"http://{CLOUD_HOST}:8080/api/nodes/{{0}}/alive";

        private const int ACCESS_POINT_ID = 1; //ESTO DEBERIA QUEMARSE PARA CADA AP
        private const int tamanioPaquete = 27;
        private const int milisLoopColaMensajes = 1000;
        private const int milisLoopAlive = 1000 * 60 * 5;

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
            bool ping = ayInternet.Ping(CLOUD_HOST);
            if (ping == false)
                Logger.Log("NO PUEDO LLEGAR AL SERVIDOR");

            // Levantamos el hilo de mensajes
            hiloMensajes = MotorDeHilos.CrearHiloLoop("EnvioMensajes", LoopColaMensajes);
            hiloMensajes.Iniciar();

            // Configuramos el Lora
            lora = new LoRaDevice();
            lora.OnReceive += Device_OnReceive;
            // Intentamos conectarnos al lora
            Hilo.Intentar(() => lora.Iniciar(), "Lora");

            // Avisamos que terminamos de configurar
            led.Write(PinValue.Low);
        }

        public override void Loop(ref bool activo)
        {
            try
            {
                ayInternet.DoPost(CrearUrl(URLkeepAlive, ACCESS_POINT_ID));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            Thread.Sleep(milisLoopAlive);
        }

        private void Device_OnReceive(object sender, SX127XDevice.OnDataReceivedEventArgs e)
        {
            try
            {
                Logger.Log($"PacketSNR: {e.PacketSnr}, PacketRSSI: {e.PacketRssi}dBm, RSSI: {e.Rssi}dBm, Length: {e.Data.Length}bytes");

                if (e.Data.Length != tamanioPaquete)
                {
                    Logger.Error($"Paquete con tamaño invalido. Esperado: {tamanioPaquete}. Recibido {e.Data.Length}.");
                    return;
                }

                if (colaMensajes.Enqueue(e.Data) != null)
                    Logger.Error("Cola desbordada!");
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

        MensajeMediciones m = new MensajeMediciones();
        private void LoopColaMensajes(ref bool activo)
        {
            // TODO: falta agrupar los mensajes por id de origen (para multiples origenes)
            // falta un mecanismo para reencolar cuando hay errores
            try
            {
                if (colaMensajes.IsEmpty())
                    return;

                m.last_updated = DateTime.UtcNow;
                m.node_measurements.Clear();

                MacAddress idOrigen = null;
                int mensajes = 0;
                while (colaMensajes.IsEmpty() == false)
                {
                    byte[] mensaje = (byte[])colaMensajes.Dequeue();
                    using (MemoryStream ms = new MemoryStream(mensaje))
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        var tipoPaquete = (TipoPaqueteEnum)br.ReadByte();
                        idOrigen = new MacAddress(br.ReadBytes(6));
                        var ticksMedicion = br.ReadInt64();
                        var bateria = br.ReadSingle();
                        var temperatura = br.ReadSingle();
                        var humedad = br.ReadSingle();
                        var fecha = new DateTime(ticksMedicion);

                        m.node_measurements.Add(new Medicion()
                        {
                            timestamp = fecha,
                            type = "bat",
                            value = bateria
                        });
                        m.node_measurements.Add(new Medicion()
                        {
                            timestamp = fecha,
                            type = "temp",
                            value = temperatura
                        });
                        m.node_measurements.Add(new Medicion()
                        {
                            timestamp = fecha,
                            type = "hum",
                            value = humedad
                        });
                    }
                    mensajes++;
                }

                ayInternet.DoPost(CrearUrl(URLaddMeasurments, ACCESS_POINT_ID), m);

                Blink(100);
                Logger.Log($"{mensajes} enviados. {colaMensajes.Count()} encolados.");
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
    }
}
