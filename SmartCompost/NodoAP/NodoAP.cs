using Equipos.SX127X;
using NanoKernel.Ayudantes;
using NanoKernel.Comunicacion;
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
        private const int tamanioCola = 75; // Valor clave tuneado para que el heap no muera, cuanto mas grande mejor, pero puede quedarse sin memoria
        private const int ventanaDesencolamiento = 20; /// Desencolamos de a pedazos, no todo junto, json es matador
        private const int clientTimeoutSeconds = 20;
        private const int intentosEnvioMediciones = 1; // Creo que no lo vamos a usar > 1
        private const int milisIntentoEnvioMediciones = 100;
        private const int segundosMedicionNodoAp = 10;
        /// ---------------------------------------------------------------
        private SmartCompostClient cliente;
        private GpioPin led;
        private LoRaDevice lora;
        private const double FREQ_LORA = 433e6; //920_000_00;
        /// ---------------------------------------------------------------
        private ConcurrentQueue colaMedicionesNodo = new ConcurrentQueue(tamanioCola);
        private ArrayList desencolados = new ArrayList();
        private MedicionesApDto medicionesAp = new MedicionesApDto();
        /// ---------------------------------------------------------------
        private MedicionesNodoDto medicionNodo;
        private Medidor medidor;
        private const string MED_TIRADOS = "tirados";
        private const string MED_ENVIADOS = "enviados";
        private const string MED_ERRORES = "errores";

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

            }, $"Wifi");

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
                },
                "Lora",
                accionException: () =>
                {
                    lora?.Dispose();
                });
            lora.OnReceive += Device_OnReceive;

            /// Mediciones del AP
            medicionNodo = new MedicionesNodoDto();
            medicionNodo.serial_number = Config.NumeroSerie;
            medicionNodo.last_updated = DateTime.UtcNow;

            // Avisamos que nos despertamos
            medicionNodo.AgregarMedicion(1, TiposMediciones.Startup);
            colaMedicionesNodo.Enqueue(medicionNodo.ToBytes());
            medicionNodo.measurements.Clear();

            // Inicializamos el medidor
            medidor = new Medidor(segundosMedicionNodoAp * 1000);
            medidor.OnMedicionesEnPeriodoCallback += Medidor_OnMedicionesEnPeriodoCallback;
            medidor.Iniciar();

            /// Avisamos que terminamos de configurar
            led.Write(PinValue.Low);
        }

        private void Medidor_OnMedicionesEnPeriodoCallback(InstanteMedicion resultado)
        {
            try
            {
                medicionNodo.AgregarMedicion(colaMedicionesNodo.Count(), TiposMediciones.TamanioCola);

                var tirados = resultado.ContadoEnPeriodo(MED_TIRADOS);
                if (tirados > 0)
                    medicionNodo.AgregarMedicion(tirados, TiposMediciones.MensajesTirados);

                var errores = resultado.ContadoEnPeriodo(MED_ERRORES);
                if (errores > 0)
                    medicionNodo.AgregarMedicion(errores, TiposMediciones.Errores);

                var enviados = resultado.ContadoEnPeriodo(MED_ENVIADOS);
                if (enviados > 0)
                    medicionNodo.AgregarMedicion(enviados, TiposMediciones.MensajesEnviados);

                var bateria = 0; // TODO MEDIR BATERIA
                if (bateria > 0)
                    medicionNodo.AgregarMedicion(bateria, TiposMediciones.Bateria);

                medicionNodo.last_updated = DateTime.UtcNow;

                colaMedicionesNodo.Enqueue(medicionNodo.ToBytes());
                Logger.Debug("Encolando mediciones del AP");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                medicionNodo.measurements.Clear();
            }
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

                byte[] medicionDesbordada = (byte[])colaMedicionesNodo.Enqueue(e.Data);
                if (medicionDesbordada == null)
                {
                    // Si lo encolo, le pongo la fecha de ahora a las mediciones
                    SetearTimestampMedicion(e.Data);
                }
                else
                {
                    medicionDesbordada = null; // Liberamos memoria, no nos interesa ya
                    medidor.Contar(MED_TIRADOS);
                    Logger.Debug("Cola mediciones desbordada");
                }
            }
            catch (Exception ex)
            {
                medidor.Contar(MED_ERRORES);
                Logger.Log(ex.Message);
            }
        }

        private static void SetearTimestampMedicion(byte[] data)
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
                int tamanioCola = colaMedicionesNodo.Count();
                for (int i = 0; i < ventanaDesencolamiento && !colaMedicionesNodo.IsEmpty(); i++)
                {
                    desencolados.Add((byte[])colaMedicionesNodo.Dequeue());
                }

                Logger.Debug($"Desencolando {desencolados.Count}/{tamanioCola} medicionesNodo");

                /// Armamos el mensaje
                foreach (byte[] item in desencolados)
                    medicionesAp.AgregarMediciones(MedicionesNodoDto.FromBytes(item));

                medicionesAp.last_updated = DateTime.UtcNow;

                bool enviado = Hilo.Intentar(
                    () => cliente.AddApMeasurments(Config.NumeroSerie, medicionesAp),
                    nombreIntento: "Envio mediciones Nodos",
                    milisIntento: milisIntentoEnvioMediciones,
                    intentos: intentosEnvioMediciones);

                if (enviado)
                {
                    Blink(100);

                    medidor.Contar(MED_ENVIADOS, desencolados.Count);
                    Logger.Log($"Se enviaron {desencolados.Count} medicionesNodo");
                }
                else
                {
                    medidor.Contar(MED_ERRORES);

                    /// Si podemos volvemos a meterlo en la cola, sino los tiro para dejar lugar a nuevos mensajes
                    int indiceReencolado = 0;
                    object obj = null;
                    do
                    {
                        obj = colaMedicionesNodo.Enqueue(desencolados[indiceReencolado++]);
                    }
                    while (obj == null && indiceReencolado < desencolados.Count);

                    medidor.Contar(MED_TIRADOS, desencolados.Count - indiceReencolado + 1);
                    Logger.Debug($"Reencolados {indiceReencolado + 1} medicionesNodo");
                }
            }
            catch (Exception e)
            {
                medidor.Contar(MED_ERRORES);
                Logger.Log(e);
            }
            finally
            {
                // Limpiamos todo
                medicionesAp.nodes_measurements.Clear();
                desencolados.Clear();
                LimpiarMemoria();

#if DEBUG
                Logger.Log($"Enviados: {medidor.ContadoTotal(MED_ENVIADOS)} | Tirados: {medidor.ContadoTotal(MED_TIRADOS)} | Encolados: {colaMedicionesNodo.Count()}");
#endif

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
