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
using System.Collections;
using System.Device.Gpio;
using System.Threading;

namespace NodoAP
{
    public class NodoAP : NodoBase
    {
        public override TiposNodo tipoNodo => TiposNodo.AccessPointLora;

        // ---------------------------------------------------------------
        // CONFIGURACION TUNEADA PARA DONGLE 4G + 10 paquetes por segundo
        private const int tamanioCola = 100;
        private const int tamanioDesencolados = 15; // Desencolamos de a pedazos, no todo junto, json es matador
        private const int segundosLoopColaMensajes = 5;
        private const int intentosEnvioMediciones = 1;
        private const int milisIntentoEnvioMediciones = 100;

        private const int clientTimeoutSeconds = 5;
        private const int segundosKeepAlive = 60;
        // ---------------------------------------------------------------

        private GpioPin led;
        private LoRaDevice lora;
        private SmartCompostClient cliente;
        private Hilo hiloMensajes;

        private ConcurrentQueue colaMedicionesNodo = new ConcurrentQueue(tamanioCola);

        private MedicionesApDto medicionesAp = new MedicionesApDto();
        private object lockColaMedicionesNodo = new object();
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
                    lock (lockColaMedicionesNodo)
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

                //if (enviandoMediciones && mensajesTirados > 0)
                //{
                //    Logger.Debug("Mensaje ignorado hasta envio de mensaje");
                //    e.Data = null;
                //    return;
                //}

                lock (lockColaMedicionesNodo)
                {
                    var mensaje = colaMedicionesNodo.Enqueue(e.Data);
                    if (mensaje != null)
                    {
                        mensaje = null;
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

        private ArrayList desencolados = new ArrayList();
        private void LoopColaMensajes(ref bool activo)
        {
            try
            {
                // Si no hay mensajes encolados no hacemos nada
                if (colaMedicionesNodo.IsEmpty())
                    return;

                // Lockeamos para poder levantar los mensajes, en ese tiempo se pueden perder interrupciones!
                int tamanioCola = 0;
                lock (lockColaMedicionesNodo)
                {
                    tamanioCola = colaMedicionesNodo.Count();
                    for (int i = 0; i < tamanioDesencolados && !colaMedicionesNodo.IsEmpty(); i++)
                    {
                        desencolados.Add((byte[])colaMedicionesNodo.Dequeue()); ;
                    }
                }
                Logger.Debug($"Desencolando {desencolados.Count}/{tamanioCola} medicionesNodo");

                // Armamos el mensaje
                foreach (byte[] item in desencolados)
                    medicionesAp.AgregarMediciones(item);
                medicionesAp.last_updated = DateTime.UtcNow;
                medicionesApJson = medicionesAp.ToJson();

                // Limpiamos el dto porque ya tenemos el json
                medicionesAp.nodes_measurements.Clear();

                enviandoMediciones = true;
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
                    m.Contar("enviados", desencolados.Count);
                    Logger.Log($"Se enviaron {desencolados.Count} medicionesNodo");
                }
                else
                {
                    // Si podemos volvemos a meterlo en la cola, sino los tiro para dejar lugar a nuevos mensajes
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
                    mensajesTirados += desencolados.Count - reencolados;
                    Logger.Debug($"Reencolados {desencolados.Count} medicionesNodo");
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
                desencolados.Clear();
                LimpiarMemoria();
                enviandoMediciones = false;

                Logger.Log($"Enviados: {m.ContadoTotal("enviados")} | Tirados: {m.ContadoTotal("tirados")} | Encolados: {colaMedicionesNodo.Count()}");

                if (colaMedicionesNodo.IsEmpty())
                {
                    // Si no hay nada en la cola, espero
                    Thread.Sleep(segundosLoopColaMensajes * 1000);
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
