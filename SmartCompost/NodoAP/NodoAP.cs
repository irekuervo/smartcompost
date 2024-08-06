using Equipos.SX127X;
using NanoKernel.Ayudantes;
using NanoKernel.Dominio;
using NanoKernel.DTOs;
using NanoKernel.Herramientas.Comunicacion;
using NanoKernel.Hilos;
using NanoKernel.Logging;
using NanoKernel.Nodos;
using System;
using System.Collections;
using System.Device.Gpio;
using System.Threading;

namespace NodoAP
{

    /// <summary>
    ///  TODO: Agregar medicion bateria
    ///  TODO: Arreglar frecuencia a una permitida por ENACOM
    /// </summary>

    public class NodoAP : NodoBase
    {
        public override TiposNodo tipoNodo => TiposNodo.AccessPointLora;

        /// ---------------------------------------------------------------
        /// CONFIGURACION TUNEADA PARA DONGLE 4G
        private const int tamanioCola = 50;
        private const int ventanaDesencolamiento = 15; /// Desencolamos de a pedazos, no todo junto, json es matador
        private const int segundosLoopColaMensajes = 5;
        private const int intentosEnvioMediciones = 1;
        private const int milisIntentoEnvioMediciones = 100;

        private const int clientTimeoutSeconds = 5;
        private const int segundosKeepAlive = 60;
        /// ---------------------------------------------------------------

        private GpioPin led;
        private LoRaDevice lora;
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
            Config.RouterSSID = "SmartCompost"; //"Bondiola 2.4";
            Config.RouterPassword = "Quericocompost"; //"conpapafritas";
            Config.SmartCompostHost = "181.88.245.34"; //"192.168.1.6";
            Config.SmartCompostPort = "8080";

            /// Configuramos el LED
            var gpio = new GpioController();
            led = gpio.OpenPin(2, PinMode.Output);
            /// Prendemos el led para avisar que estamos configurando
            led.Write(PinValue.High);

            /// Conectamos a internet
            Hilo.Intentar(() =>
            {
                ayInternet.ConectarsePorWifi(Config.RouterSSID, Config.RouterPassword);

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

            Hilo.Intentar(() =>
            {
                cliente.NodeStartup(Config.NumeroSerie);
                Logger.Log($"Cliente creado a {Config.SmartCompostHost}:{Config.SmartCompostPort}");
            }, "Startup");

            /// Configuramos el Lora
            Hilo.Intentar(() =>
            {
                lora = new LoRaDevice();
                lora.Iniciar();
                Logger.Log("Lora conectado");
            }, "Lora");
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

                MedicionesNodoDto medicionDesbordada = null;
                lock (lockColaMedicionesNodo)
                {
                    try
                    {
                        medicionDesbordada = (MedicionesNodoDto)colaMedicionesNodo.Enqueue(MedicionesNodoDto.FromBytes(e.Data));
                    }
                    catch (Exception ex)
                    {
                        mensajesTiradosPeriodo++;
                        Logger.Log(ex);
                    }
                    finally
                    {
                        e.Data = null;
                    }
                }

                if (medicionDesbordada != null)
                {
                    /// lo liberamos de la memoria
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

        // ------- DESENCOLAMIENTO ----------
        public override void Loop(ref bool activo)
        {
            try
            {
                /// Si no hay mensajes encolados no hacemos nada
                if (colaMedicionesNodo.IsEmpty())
                    return;

                /// Lockeamos para poder levantar los mensajes, en ese tiempo se pueden perder interrupciones!
                int tamanioCola = 0;
                lock (lockColaMedicionesNodo)
                {
                    tamanioCola = colaMedicionesNodo.Count();
                    for (int i = 0; i < ventanaDesencolamiento && !colaMedicionesNodo.IsEmpty(); i++)
                    {
                        desencolados.Add(colaMedicionesNodo.Dequeue());
                    }
                }
                Logger.Debug($"Desencolando {desencolados.Count}/{tamanioCola} medicionesNodo");

                /// Armamos el mensaje
                foreach (MedicionesNodoDto item in desencolados)
                    medicionesAp.AgregarMediciones(item);

                // --------------------------------------------------------------------------------

                // TODO: Agregar medicion bateria!!!

                // --------------------------------------------------------------------------------

                medicionesAp.last_updated = DateTime.UtcNow;
                medicionesApJson = medicionesAp.ToJson();

                /// Limpiamos el dto porque ya tenemos el json
                medicionesAp.nodes_measurements.Clear();

                bool enviado = Hilo.Intentar(
                    () => cliente.AddApMeasurments(Config.NumeroSerie, medicionesApJson),
                    nombreIntento: "Envio mediciones AP",
                    milisIntento: milisIntentoEnvioMediciones,
                    intentos: intentosEnvioMediciones);

                /// Limpiamos el json
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
                Logger.Debug($"Enviados: {enviadosTotal} | Tirados: {tiradosTotal} | Encolados: {colaMedicionesNodo.Count()}");

                if (colaMedicionesNodo.Count() < ventanaDesencolamiento)
                    Thread.Sleep(segundosLoopColaMensajes * 1000);
            }
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
