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
using System.Device.Adc;
using System.Device.Gpio;
using System.IO;
using System.Threading;

namespace NodoAP
{
    public class NodoAP : NodoBase
    {
        public override TiposNodo tipoNodo => TiposNodo.AccessPointLora;

        /// ---------------------------------------------------------------
        /// CONFIGURACION TUNEADA PARA DONGLE 4G
        private const int tamanioCola = 75; // Valor clave tuneado para que el heap no muera, cuanto mas grande mejor, pero puede quedarse sin memoria
        private const int ventanaDesencolamiento = 20; /// Desencolamos de a pedazos, no todo junto, json es matador
        private const int clientTimeoutSeconds = 20;
        private const int intentosEnvioMediciones = 1; // Por alguna razon anda mejor en 1
        private const int milisIntentoEnvioMediciones = 100;
        private const int segundosTelemetriaNodoAp = 10 * 1;
        /// ---------------------------------------------------------------

        private LoRaDevice lora;
        private const int LORA_PIN_MISO = 19;
        private const int LORA_PIN_MOSI = 23;
        private const int LORA_PIN_CLK = 18;
        private const int LORA_PIN_NSS = 5;
        private const int LORA_PIN_DIO0 = 25;
        private const int LORA_PIN_RESET = 14;
        private const double LORA_FREQ = 433e6;
        /// ---------------------------------------------------------------
        private const int ADC_CHANNEL_BATERIA = 6;  //pin 39        // ADC Channel 6 - GPIO 34
        private AdcController adcController;
        private AdcChannel adcBateria;
        /// ---------------------------------------------------------------
        private ConcurrentQueue colaMedicionesNodo = new ConcurrentQueue(tamanioCola);
        private ArrayList mensajesDesencolados = new ArrayList();
        private MedicionesApDto payloadEnvioMediciones = new MedicionesApDto();
        /// ---------------------------------------------------------------
        private SmartCompostClient cliente;
        private GpioPin led;
        private MedicionesNodoDto medicionNodo;
        private Medidor medidor;
        private int mensajesMedicionAP = 0;
        private const string M_TIRADOS = "tirados";
        private const string M_RECIBIDOS = "encolados";
        private const string M_ENVIADOS = "enviados";
        private const string ERRORES = "errores";

        public override void Setup()
        {
            // TODO: Esto deberia hacerse con el deploy, no hardcodearse
            Config.RouterSSID = "SmartCompost";
            Config.RouterPassword = "Quericocompost";
            Config.SmartCompostHost = "smartcompost.net";
            Config.SmartCompostPort = "8080";
            Config.NumeroSerie = "58670345-7dc4-11ef-919e-0242ac160004";

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
                    throw new Exception("Fallo la conexion al router");
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
                    lora = new LoRaDevice(
                        pinMISO: LORA_PIN_MISO,
                        pinMOSI: LORA_PIN_MOSI,
                        pinSCK: LORA_PIN_CLK,
                        pinNSS: LORA_PIN_NSS,
                        pinDIO0: LORA_PIN_DIO0,
                        pinReset: LORA_PIN_RESET);
                    lora.Iniciar(LORA_FREQ);
                    lora.ModoRecibir();
                },
                "Lora",
                accionException: () =>
                {
                    lora?.Dispose();
                });

            /// Mediciones del AP
            medicionNodo = new MedicionesNodoDto();
            medicionNodo.serial_number = Config.NumeroSerie;
            medicionNodo.last_updated = DateTime.UtcNow;

            // Avisamos que nos despertamos
            medicionNodo.AgregarMedicion(1, TiposMediciones.Startup);
            Hilo.Intentar(() => cliente.AddNodeMeasurments(Config.NumeroSerie, medicionNodo), intentos: 3);
            medicionNodo.measurements.Clear();

            // Inicializamos el medidor del ap
            adcController = new AdcController();
            adcBateria = adcController.OpenChannel(ADC_CHANNEL_BATERIA);
            medidor = new Medidor(segundosTelemetriaNodoAp * 1000);
            medidor.OnMedicionesEnPeriodoCallback += Medidor_OnTelemetriaEnPeriodoCallback;
            medidor.Iniciar();

            // Escuchamos cuando terminamos de configurar
            lora.OnReceive += Lora_OnReceive;

            /// Avisamos que terminamos de configurar
            led.Write(PinValue.Low);
        }

        #region RECEPCION DE MENSAJES
        private void Lora_OnReceive(object sender, SX127XDevice.OnDataReceivedEventArgs e)
        {
            try
            {
                if (e.Data == null)
                    throw new Exception("Mensaje null recibido");

                Logger.Log($"{e.Data.Length} bytes recibidos");
                medidor.Contar(M_RECIBIDOS);

                byte[] mensaje = ValidarYProcesarMensaje(e.Data);
                if (mensaje == null)
                {
                    e.Data = null;
                    medidor.Contar(ERRORES);
                    return;
                }

                byte[] mensajeDesbordado = (byte[])colaMedicionesNodo.Enqueue(mensaje);
                if (mensajeDesbordado != null)
                {
                    mensajeDesbordado = null;
                    medidor.Contar(M_TIRADOS);
                    Logger.Debug("Cola mediciones desbordada");
                }
            }
            catch (Exception ex)
            {
                medidor.Contar(ERRORES);
                Logger.Log(ex.Message);
            }
        }

        private byte[] ValidarYProcesarMensaje(byte[] data)
        {
            // Le clavamos la hora de arrivo como la hora de medicion, es la mejor aproximacion que tenemos
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader br = new BinaryReader(ms))
            {
                var tipoPaquete = (TipoPaqueteEnum)br.ReadByte();
                if (tipoPaquete != TipoPaqueteEnum.MedicionNodo)
                {
                    Logger.Error($"Mensaje {tipoPaquete} no soportado");
                    return null;
                }

                // Pisamos la fecha ya que asumimos que la medicion no tiene una fecha valida todavia (deberia)
                MedicionesNodoDto.SetearTimestamp(data, ms, br);
                return data;
            }
        }
        #endregion

        #region ENVIO DE MENSAJES
        public override void ColaLoop(ref bool activo)
        {
            if (colaMedicionesNodo.IsEmpty())
            {
                /// Si no hay mensajes encolados liberamos el thread
                Thread.Sleep(100);
                return;
            }

            try
            {
                int tamanioCola = colaMedicionesNodo.Count();
                int desencolados = 0;
                for (int i = 0; i < ventanaDesencolamiento && !colaMedicionesNodo.IsEmpty(); i++)
                {
                    var item = (byte[])colaMedicionesNodo.Dequeue();
                    try
                    {
                        payloadEnvioMediciones.AgregarMediciones(MedicionesNodoDto.FromBytes(item));
                        mensajesDesencolados.Add(item);
                        desencolados++;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                }

                if (desencolados == 0)
                {
                    medidor.Contar(ERRORES);
                    Logger.Error("No se pudo deserealizar nada");
                    return;
                }

                Logger.Debug($"{desencolados}/{tamanioCola} mensajes desencolados");

                // Envio del payload
                payloadEnvioMediciones.last_updated = DateTime.UtcNow;
                bool payloadEnviado = Hilo.Intentar(
                    () => cliente.AddApMeasurments(Config.NumeroSerie, payloadEnvioMediciones),
                    nombreIntento: "Envio payload AP",
                    milisIntento: milisIntentoEnvioMediciones,
                    intentos: intentosEnvioMediciones);

                if (payloadEnviado)
                {
                    Blink(100);

                    medidor.Contar(M_ENVIADOS, desencolados - Interlocked.Exchange(ref mensajesMedicionAP, mensajesMedicionAP));
                    Logger.Log($"{desencolados} mensajes enviados");
                }
                else
                {
                    medidor.Contar(ERRORES);

                    /// Si podemos volvemos a meterlo en la cola, sino los tiro para dejar lugar a nuevos mensajes
                    int indiceReencolado = 0;
                    object obj = null;
                    do
                    {
                        obj = colaMedicionesNodo.Enqueue(mensajesDesencolados[indiceReencolado++]);
                    }
                    while (obj == null && indiceReencolado < desencolados);

                    medidor.Contar(M_TIRADOS, desencolados - indiceReencolado + 1);
                    Logger.Debug($"{indiceReencolado + 1} mensajes reencolados");
                }
            }
            catch (Exception e)
            {
                medidor.Contar(ERRORES);
                Logger.Log(e);
            }
            finally
            {
                // Limpiamos toda la memoria posible
                Interlocked.Exchange(ref mensajesMedicionAP, 0);
                payloadEnvioMediciones.nodes_measurements.Clear();
                mensajesDesencolados.Clear();
                LimpiarMemoria();

                Logger.Debug($"Enviados: {medidor.ContadoTotal(M_ENVIADOS)} | Tirados: {medidor.ContadoTotal(M_TIRADOS)} | Encolados: {colaMedicionesNodo.Count()}");
            }
        }
        #endregion

        #region MEDICIONES AP
        private void Medidor_OnTelemetriaEnPeriodoCallback(InstanteMedicion resultado)
        {
            try
            {
                medicionNodo.AgregarMedicion(colaMedicionesNodo.Count(), TiposMediciones.TamanioCola);

                var recibidos = resultado.ContadoEnPeriodo(M_RECIBIDOS);
                if (recibidos > 0)
                    medicionNodo.AgregarMedicion(recibidos, TiposMediciones.MensajesRecibidos);

                var tirados = resultado.ContadoEnPeriodo(M_TIRADOS);
                if (tirados > 0)
                    medicionNodo.AgregarMedicion(tirados, TiposMediciones.MensajesTirados);

                var errores = resultado.ContadoEnPeriodo(ERRORES);
                if (errores > 0)
                    medicionNodo.AgregarMedicion(errores, TiposMediciones.Errores);

                var enviados = resultado.ContadoEnPeriodo(M_ENVIADOS);
                if (enviados > 0)
                    medicionNodo.AgregarMedicion(enviados, TiposMediciones.MensajesEnviados);

                var bateria = MedirBateria();
                medicionNodo.AgregarMedicion(bateria, TiposMediciones.Bateria);

                medicionNodo.last_updated = DateTime.UtcNow;

                colaMedicionesNodo.Enqueue(medicionNodo.ToBytes());

                Interlocked.Increment(ref mensajesMedicionAP);

                Logger.Debug("Encolando mediciones del AP");
            }
            catch (Exception ex)
            {
                medidor.Contar(ERRORES);
                Logger.Log(ex);
            }
            finally
            {
                medicionNodo.measurements.Clear();
            }
        }

        private float MedirBateria()
        {
            int analogValue = adcBateria.ReadValue();
            float vSensor = analogValue / 4095f * 3.3f;

            // Cuenta de la bateria, mapeando las cotas con el ADC
            // y = a x + b
            // 0 = a * 2.52 V + b
            // 100 = a * 3.3 V + b
            // y = 128.21 * x − 323.06
            float bateriaPorcentaje = 128.21f * vSensor - 323.06f;

            return bateriaPorcentaje.ToPorcentaje();
        }

        #endregion

        private void Blink(int time)
        {
            led.Write(PinValue.High);
            Thread.Sleep(time);
            led.Write(PinValue.Low);
        }
    }
}
