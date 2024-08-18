using Equipos.SX127X;
using NanoKernel.Ayudantes;
using NanoKernel.Comunicacion;
using NanoKernel.Dominio;
using NanoKernel.DTOs;
using NanoKernel.Herramientas.Comunicacion;
using NanoKernel.Hilos;
using NanoKernel.Logging;
using NanoKernel.Nodos;
using System;
using System.Collections;
using System.Device.Gpio;
using System.IO;
using System.Threading;

namespace NodoAP
{
    /// <summary>
    /// 
    ///  TODO: Agregar medicion bateria
    ///  TODO: Arreglar frecuencia a una permitida por ENACOM
    ///  
    /// </summary>

    public class NodoAP : NodoBase
    {
        public override TiposNodo tipoNodo => TiposNodo.AccessPointLora;

        /// ---------------------------------------------------------------
        /// CONFIGURACION TUNEADA PARA DONGLE 4G
        private const int tamanioCola = 75;
        private const int ventanaDesencolamiento = 20; /// Desencolamos de a pedazos, no todo junto, json es matador
        private const int segundosLoopColaMensajes = 5;
        private const int clientTimeoutSeconds = 20;

        // Creo que no lo vamos a usar
        private const int intentosEnvioMediciones = 1;
        private const int milisIntentoEnvioMediciones = 100;
        /// ---------------------------------------------------------------

        private GpioPin led;
        private LoRaDevice lora;
        private const double FRECUENCIA = 433e6; //920_000_00;

        private SmartCompostClient cliente;

        private ConcurrentQueue colaMedicionesNodo = new ConcurrentQueue(tamanioCola);

        private MedicionesApDto medicionesAp = new MedicionesApDto();
        private ArrayList desencolados = new ArrayList();
        private Random rnd = new Random();
        private MedicionDto medicionBateria = new MedicionDto() { type = TiposMediciones.Bateria.GetString() };

        private object lockColaMedicionesNodo = new object();
        private string medicionesApJson;
        private int mensajesTiradosPeriodo = 0;

        private int enviadosTotal = 0;
        private int tiradosTotal = 0;
        //private Medidor m = new Medidor();

        public override void Setup()
        {
            // ES: BORRAR!!!!!! Estoy probando en mi casa
            Config.RouterSSID = "Bondiola 2.4"; // "SmartCompost"; //
            Config.RouterPassword = "conpapafritas";  //"Quericocompost"; //
            Config.SmartCompostHost = "smartcompost.net"; //"181.88.245.34"; //"192.168.1.6";
            Config.SmartCompostPort = "8080";
            Config.NumeroSerie = "7e0674f0-5451-11ef-92ae-0242ac140004";

            /// Configuramos el LED
            var gpio = new GpioController();
            led = gpio.OpenPin(2, PinMode.Output);
            /// Prendemos el led para avisar que estamos configurando
            led.Write(PinValue.High);

            /// Conectamos a internet
            Hilo.Intentar(() =>
            {
                if (ayInternet.ConectarsePorWifi(Config.RouterSSID, Config.RouterPassword) == false)
                {
                    throw new Exception("No hay internet");
                }

                string ip = ayInternet.ObtenerIp();
                if (ip == "0.0.0.0")
                    throw new Exception("No se pudo asignar la ip");

                /// IP asignada
                Logger.Log($"Ip asignada: {ip}");

            }, $"Conectando Wifi {Config.RouterSSID} - {Config.RouterPassword}");

            /// Vemos si podemos pingear la api
            bool ping = ayInternet.Ping(Config.SmartCompostHost);
            if (ping == false)
                Logger.Log("NO HAY PING AL SERVIDOR");

            /// Cliente
            cliente = new SmartCompostClient(Config.SmartCompostHost, Config.SmartCompostPort, clientTimeoutSeconds);

            /// Configuramos el Lora
            Hilo.Intentar(() =>
            {
                lora = new LoRaDevice();
                lora.Iniciar();
                Logger.Log("Lora conectado");
            }, "Lora", accionException: () =>
            {
                lora?.Dispose();
            });
            lora.OnReceive += Device_OnReceive;

            /// Avisamos que terminamos de configurar
            led.Write(PinValue.Low);
        }

        // ------- ENCOLAMIENTO ----------
        private void Device_OnReceive(object sender, SX127XDevice.OnDataReceivedEventArgs e)
        {
            try
            {
                //Logger.Debug($"PacketSNR: {e.PacketSnr}, PacketRSSI: {e.PacketRssi}dBm, RSSI: {e.Rssi}dBm, Length: {e.Data.Length}bytes");
                Logger.Debug($"Paquete recibido: {e.Data.Length} bytes");

                if (e.Data == null)
                    return;

                byte[] medicionDesbordada = null;
                lock (lockColaMedicionesNodo)
                {
                    medicionDesbordada = (byte[])colaMedicionesNodo.Enqueue(e.Data);
                }

                if (medicionDesbordada == null)
                {
                    // Si lo encolo, le pongo la fecha de ahora
                    SetearTimestamp(e.Data);
                }
                else
                {
                    medicionDesbordada = null;
                    mensajesTiradosPeriodo++;
                    Logger.Debug("Cola mediciones desbordada");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        private static void SetearTimestamp(byte[] data)
        {
            // Le clavamos la hora de arrivo como la hora de medicion, es la mejor aproximacion que tenemos
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader br = new BinaryReader(ms))
            {
                //movemos la posicion del buffer hasta la fecha, y la cambiamos
                var tipoPaquete = (TipoPaqueteEnum)br.ReadByte();
                if (tipoPaquete == TipoPaqueteEnum.MedicionNodo)
                {
                    br.ReadString();
                    Array.Copy(BitConverter.GetBytes(DateTime.UtcNow.Ticks), 0, data, (int)ms.Position, sizeof(long));
                }
            }
        }

        // ------- DESENCOLAMIENTO ----------
        public override void Loop(ref bool activo)
        {
            /// Si no hay mensajes encolados no hacemos nada
            if (colaMedicionesNodo.IsEmpty())
            {
                Thread.Sleep(100);
                return;
            }

            try
            {
                /// Lockeamos para poder levantar los mensajes, en ese tiempo se pueden perder interrupciones!
                int tamanioCola = 0;
                lock (lockColaMedicionesNodo)
                {
                    tamanioCola = colaMedicionesNodo.Count();
                    for (int i = 0; i < ventanaDesencolamiento && !colaMedicionesNodo.IsEmpty(); i++)
                    {
                        desencolados.Add((byte[])colaMedicionesNodo.Dequeue()); ;
                    }
                }

                Logger.Debug($"Desencolando {desencolados.Count}/{tamanioCola} medicionesNodo");

                /// Armamos el mensaje
                foreach (byte[] item in desencolados)
                    medicionesAp.AgregarMediciones(MedicionesNodoDto.FromBytes(item));

                // TODO: Agregar medicion bateria!!!

                medicionesAp.last_updated = DateTime.UtcNow;
                medicionesApJson = medicionesAp.ToJson();

                /// Limpiamos el dto porque ya tenemos el json
                medicionesAp.nodes_measurements.Clear();

                bool enviado = Hilo.Intentar(
                    () => cliente.AddApMeasurments(Config.NumeroSerie, medicionesApJson),
                    nombreIntento: "Envio mediciones AP",
                    milisIntento: milisIntentoEnvioMediciones,
                    intentos: intentosEnvioMediciones);

                // Limpiamos el json
                medicionesApJson = null;

                if (enviado)
                {
                    Blink(100);

                    //m.Contar("enviados", desencolados.Count);
                    enviadosTotal += desencolados.Count;

                    Logger.Log($"Se enviaron {desencolados.Count} medicionesNodo");
                }
                else
                {
                    /// Si podemos volvemos a meterlo en la cola, sino los tiro para dejar lugar a nuevos mensajes
                    int remaining = colaMedicionesNodo.Size() - colaMedicionesNodo.Count();
                    int reencolados = 0;
                    lock (lockColaMedicionesNodo)
                    {
                        for (int i = 0; i < remaining && i < desencolados.Count; i++)
                        {
                            colaMedicionesNodo.Enqueue(desencolados[i]);
                            reencolados++;
                        }
                    }
                    mensajesTiradosPeriodo += desencolados.Count - reencolados;
                    Logger.Debug($"Reencolados {desencolados.Count} medicionesNodo");
                }

                if (mensajesTiradosPeriodo > 0)
                {
                    //m.Contar("tirados", mensajesTirados);
                    tiradosTotal += mensajesTiradosPeriodo;

                    Logger.Error($"Se perdieron {mensajesTiradosPeriodo} medicionesNodo");
                    mensajesTiradosPeriodo = 0;
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
            finally
            {
                desencolados.Clear();

                LimpiarMemoria();

                //Logger.Log($"Enviados: {m.ContadoTotal("enviados")} | Tirados: {m.ContadoTotal("tirados")} | Encolados: {colaMedicionesNodo.Count()}");
                Logger.Log($"Enviados: {enviadosTotal} | Tirados: {tiradosTotal} | Encolados: {colaMedicionesNodo.Count()}");

                //if (colaMedicionesNodo.Count() < ventanaDesencolamiento)
                //    Thread.Sleep(segundosLoopColaMensajes * 1000);
            }
        }

        private void Blink(int time)
        {
            led.Write(PinValue.High);
            Thread.Sleep(time);
            led.Write(PinValue.Low);
        }
    }
}
