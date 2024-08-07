using Equipos.SX127X;
using System;
using System.Collections;
using System.Device.Gpio;
using System.Threading;

namespace PruebaAP
{
    public class Program
    {
        /// ---------------------------------------------------------------
        /// CONFIGURACION TUNEADA PARA DONGLE 4G + 10 paquetes por segundo
        private const int tamanioCola = 100;
        private const int ventanaDesencolamiento = 15; /// Desencolamos de a pedazos, no todo junto, json es matador
        private const int segundosLoopColaMensajes = 5;
        private const int intentosEnvioMediciones = 1;
        private const int milisIntentoEnvioMediciones = 100;

        private const int clientTimeoutSeconds = 5;
        private const int segundosKeepAlive = 60;
        /// ---------------------------------------------------------------

        private static ConfigNodo Config = ConfigNodo.Default();

        private static GpioPin led;
        private static LoRaDevice lora;
        private static SmartCompostClient cliente;

        private static ConcurrentQueue colaMedicionesNodo = new ConcurrentQueue(tamanioCola);

        private static MedicionesApDto medicionesAp = new MedicionesApDto();
        private static ArrayList desencolados = new ArrayList();

        private static object lockColaMedicionesNodo = new object();
        private static string medicionesApJson;
        private static bool enviandoMediciones = false;
        private static int mensajesTiradosPeriodo = 0;

        private static int enviadosTotal = 0;
        private static int tiradosTotal = 0;
        //private Medidor m = new Medidor();


        Random rnd = new Random();
        MedicionDto medicionBateria = new MedicionDto() { type = TiposMediciones.Bateria.GetString() };

        public static void Main()
        {
            Setup();

            while (true)
            {
                Loop();
            }
        }

        private static void Setup()
        {
            // ES: BORRAR!!!!!! Estoy probando en mi casa
            Config.RouterSSID = "SmartCompost"; //"Bondiola 2.4";
            Config.RouterPassword = "Quericocompost"; //"conpapafritas";
            Config.SmartCompostHost = "smartcompost.net"; //"181.88.245.34"; //"192.168.1.6";
            Config.SmartCompostPort = "8080";

            /// Configuramos el LED
            var gpio = new GpioController();
            led = gpio.OpenPin(2, PinMode.Output);
            /// Prendemos el led para avisar que estamos configurando
            led.Write(PinValue.High);

            /// Conectamos a internet
            Intentar(() =>
            {
                ayInternet.ConectarsePorWifi(Config.RouterSSID, Config.RouterPassword);

                string ip = ayInternet.ObtenerIp();
                if (ip == "0.0.0.0")
                    throw new Exception("No se pudo asignar la ip");

                /// IP asignada
                Logger.Log(ip);
            }, $"Config Wifi: {Config.RouterSSID}");

            /// Vemos si podemos pingear la api
            //bool ping = ayInternet.Ping(Config.SmartCompostHost);
            //if (ping == false)
            //    Logger.Log("NO HAY PING AL SERVIDOR");

            /// Cliente
            cliente = new SmartCompostClient(Config.SmartCompostHost, Config.SmartCompostPort, clientTimeoutSeconds);

            Intentar(() =>
            {
                cliente.NodeStartup(Config.NumeroSerie);
            }, "Startup");

            /// Configuramos el Lora
            Intentar(() =>
            {
                lora = new LoRaDevice();
                lora.Iniciar();
            }, "Lora");
            lora.OnReceive += Device_OnReceive;

            /// Avisamos que terminamos de configurar
            led.Write(PinValue.Low);
        }


        private static void Loop()
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
                        desencolados.Add((byte[])colaMedicionesNodo.Dequeue()); ;
                    }
                }
                Logger.Debug($"Desencolando {desencolados.Count}/{tamanioCola} medicionesNodo");

                /// Armamos el mensaje
                foreach (byte[] item in desencolados)
                    medicionesAp.AgregarMediciones(item);

                // TODO: Agregar medicion bateria!!!

                medicionesAp.last_updated = DateTime.UtcNow;
                medicionesApJson = medicionesAp.ToJson();

                Logger.Debug(medicionesApJson);

                /// Limpiamos el dto porque ya tenemos el json
                medicionesAp.nodes_measurements.Clear();

                enviandoMediciones = true;
                bool enviado = Intentar(
                    () => cliente.AddApMeasurments("7e0674f0-5451-11ef-92ae-0242ac140004"/*Config.NumeroSerie*/, medicionesApJson),
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
                enviandoMediciones = false;

                //Logger.Log($"Enviados: {m.ContadoTotal("enviados")} | Tirados: {m.ContadoTotal("tirados")} | Encolados: {colaMedicionesNodo.Count()}");
                Logger.Log($"Enviados: {enviadosTotal} | Tirados: {tiradosTotal} | Encolados: {colaMedicionesNodo.Count()}");

                if (colaMedicionesNodo.Count() < ventanaDesencolamiento)
                    Thread.Sleep(segundosLoopColaMensajes * 1000);
            }
        }

        private static void Device_OnReceive(object sender, SX127XDevice.OnDataReceivedEventArgs e)
        {
            try
            {
                Logger.Debug($"PacketSNR: {e.PacketSnr}, PacketRSSI: {e.PacketRssi}dBm, RSSI: {e.Rssi}dBm, Length: {e.Data.Length}bytes");

                byte[] mensajeDesbordado = null;
                lock (lockColaMedicionesNodo)
                {
                    mensajeDesbordado = (byte[])colaMedicionesNodo.Enqueue(e.Data);
                }

                if (mensajeDesbordado != null)
                {
                    /// lo liberamos de la memoria
                    mensajeDesbordado = null;
                    mensajesTiradosPeriodo++;
                    Logger.Debug("Cola mediciones desbordada");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        private static void Blink(int time)
        {
#if DEBUG
            led.Write(PinValue.High);
            Thread.Sleep(time);
            led.Write(PinValue.Low);
#endif
        }

        private static bool Intentar(Action accion, string nombreIntento = "", int milisIntento = 1000, uint intentos = uint.MaxValue)
        {
            Logger.Debug("Intentando " + nombreIntento);
            var ok = false;
            while (!ok && intentos > 0)
            {
                try
                {
                    accion.Invoke();
                    Logger.Debug($"{nombreIntento} OK");
                    ok = true;
                }
                catch (Exception)
                {
                    Logger.Debug("Reintentando");
                    ok = false;
                }
                finally
                {
                    intentos--;

                    if (!ok && intentos > 0)
                        Thread.Sleep(milisIntento);
                }
            }

            return ok;
        }

        private static void LimpiarMemoria()
        {
            //uint freeMemory = nanoFramework.Runtime.Native.GC.Run(false);
            //EstadisticaRAM.AgregarMuestra(freeMemory);
            //Logger.Debug($"Free RAM: {EstadisticaRAM.UltimaMuestra} | Max: {EstadisticaRAM.Maximo} | Min: {EstadisticaRAM.Minimo} | mu: {EstadisticaRAM.Promedio()} sigma: {EstadisticaRAM.Desvio()} ");
        }
    }
}
