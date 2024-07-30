using Equipos.SX127X;
using NanoKernel.Ayudantes;
using NanoKernel.Dominio;
using NanoKernel.DTOs;
using NanoKernel.Herramientas.Comunicacion;
using NanoKernel.Herramientas.Medidores;
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

        // ---------------------------------------------------------------
        private const int segundosKeepAlive = 60;
        private const int tamanioCola = 25;
        private const int segundosLoopColaMensajes = 10;
        private const int intentosEnvioMediciones = 1;
        private const int milisIntentoEnvioMediciones = 100;
        private const int clientTimeoutSeconds = 5;
        // ---------------------------------------------------------------

        private GpioPin led;
        private LoRaDevice lora;
        private SmartCompostClient cliente;
        private Hilo hiloMensajes;

        private ConcurrentQueue colaMedicionesNodo = new ConcurrentQueue(tamanioCola);

        private MedicionesApDto medicionesAp = new MedicionesApDto();
        private object lockMensaje = new object();
        private string medicionesApJson;
        private bool enviandoMediciones = false;
        private int mensajesTirados = 0;

        private Medidor m = new Medidor();

        public override void Setup()
        {
            // Configuramos el LED
            var gpio = new GpioController();
            led = gpio.OpenPin(2, PinMode.Output);
            // Prendemos el led para avisar que estamos configurando
            led.Write(PinValue.High);

            // Conectamos a internet
            Hilo.Intentar(() =>
            {
                ayInternet.ConectarsePorWifi(Config.RouterSSID, Config.RouterPassword);

                string ip = ayInternet.ObtenerIp();
                if (ip == "0.0.0.0")
                    throw new Exception("No se pudo asignar la ip");

                // IP asignada
                Logger.Log(ip);
            }, $"Config Wifi: {Config.RouterSSID}");

            // Vemos si podemos pingear la api
            bool ping = ayInternet.Ping(Config.SmartCompostHost);
            if (ping == false)
                Logger.Log("NO HAY PING AL SERVIDOR");

            // Cliente
            cliente = new SmartCompostClient(Config.SmartCompostHost, Config.SmartCompostPort, clientTimeoutSeconds);

            Hilo.Intentar(() =>
            {
                cliente.NodeStartup(Config.NumeroSerie);
            }, "Startup");

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

        Random rnd = new Random();
        MedicionDto medicionBateria = new MedicionDto() { type = TiposMediciones.Bateria.GetString() };
        public override void Loop(ref bool activo)
        {
            try
            {
                if ((DateTime.UtcNow - cliente.UltimoRequest).TotalSeconds > segundosKeepAlive && !enviandoMediciones)
                {
                    lock (lockMensaje)
                    {
                        medicionBateria.timestamp = DateTime.UtcNow;
                        //TODO: MEDIR DE VERDAD
                        medicionBateria.value = (float)rnd.NextDouble();
                        medicionesAp.AgregarMedicion(Config.NumeroSerie, medicionBateria);
                    }
                }
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
                    if (colaMedicionesNodo.Enqueue(e.Data) != null)
                    {
                        mensajesTirados++;
                        Logger.Debug("Cola mediciones desbordada");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        int medicionesNodo = 0;
        private void LoopColaMensajes(ref bool activo)
        {
            try
            {
                // Si no hay mensajes encolados no hacemos nada
                if (colaMedicionesNodo.IsEmpty())
                    return;

                // Lockeamos para poder levantar los mensajes, en ese tiempo se pueden perder interrupciones!
                lock (lockMensaje)
                {
                    medicionesNodo = colaMedicionesNodo.Count();
                    Logger.Debug($"Desencolando {medicionesNodo} medicionesNodo");
                    foreach (var item in colaMedicionesNodo.GetItems())
                    {
                        medicionesAp.AgregarMediciones((byte[])item);
                    }

                    colaMedicionesNodo.Clear();
                }
                medicionesAp.last_updated = DateTime.UtcNow;

                // Pasamos al payload asi liberamos memoria rapido
                medicionesApJson = medicionesAp.ToJson();
                medicionesAp.nodes_measurements.Clear();

                // Flag para no pisarnos con el keep alive
                enviandoMediciones = true;
                bool enviado = Hilo.Intentar(
                    () => cliente.AddApMeasurments(Config.NumeroSerie, medicionesApJson),
                    nombreIntento: "Envio mediciones AP",
                    milisIntento: milisIntentoEnvioMediciones,
                    intentos: intentosEnvioMediciones);

                // Limpiamos memoria
                medicionesApJson = null;

                if (enviado)
                {
                    Blink(100);
                    m.Contar("enviados", medicionesNodo);
                    Logger.Log($"Se enviaron {medicionesNodo} medicionesNodo");
                }
                else
                {
                    mensajesTirados += medicionesNodo;
                }

                if (mensajesTirados > 0)
                {
                    m.Contar("tirados", mensajesTirados);
                    Logger.Error($"Se perdieron {mensajesTirados} medicionesNodo");
                    mensajesTirados = 0;
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
            finally
            {
                LimpiarMemoria();
                enviandoMediciones = false;

                Logger.Log($"Enviados: {m.ContadoTotal("enviados")} | Tirados: {m.ContadoTotal("tirados")}");

                if (colaMedicionesNodo.IsEmpty())
                {
                    // Si no hay nada en la cola, espero
                    Thread.Sleep(segundosLoopColaMensajes * 1000);
                }
                else
                {
                    // Si en el medio se encolaron cosas, me fijo si hace falta esperar
                    int sleep = (int)(cliente.UltimoRequest.AddSeconds(segundosLoopColaMensajes) - DateTime.UtcNow).TotalMilliseconds;
                    if (sleep > 0)
                        Thread.Sleep(sleep);
                }
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
